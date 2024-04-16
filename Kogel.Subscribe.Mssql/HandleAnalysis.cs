using Kogel.Subscribe.Mssql.Entites;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Kogel.Subscribe.Mssql.Middleware;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 解析
    /// </summary>
    public sealed class HandleAnalysis<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// 订阅通知
        /// </summary>
        /// <param name="messageList"></param>
        private delegate void SubscribeHandler(List<SubscribeMessage<T>> messageList);

        /// <summary>
        /// 事件总线
        /// </summary>
        private event SubscribeHandler _eventBus;

        /// <summary>
        /// 上下文对象
        /// </summary>
        private SubscribeContext<T> _context;

        /// <summary>
        /// 
        /// </summary>
        private MiddlewareSubscribe<T> _middlewareSubscribe;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="options"></param>
        /// <param name="shardsTable"></param>
        /// <returns></returns>
        internal HandleAnalysis<T> Register(ISubscribe<T> handler, OptionsBuilder<T> options, string shardsTable = null)
        {
            //定义上下文对象
            this._context = new SubscribeContext<T>(options, shardsTable ?? typeof(T).GetTableName());

            //注册中间件
            this._middlewareSubscribe = new MiddlewareSubscribe<T>(_context).Register();
            _eventBus += new SubscribeHandler(_middlewareSubscribe.Subscribes);

            //自定义注册
            _eventBus += new SubscribeHandler(handler.Subscribes);

            //开始处理
            this.Handler();
            return this;
        }

        /// <summary>
        /// 处理中心
        /// </summary>
        internal void Handler()
        {
            //首次进入确认
            CheckFirst();

            //开始循环扫描
            while (true)
            {
                //开始扫描全表
                List<CT<T>> changeData = CheckFirstScanFull();
                if (changeData == null)
                {
                    //表扫描完成
#if DEBUG
                    Console.WriteLine($"开始检查表{_context.TableName}变更");
#endif
                    //检查变更
                    changeData = CheckChange();
#if DEBUG
                    Console.WriteLine($"检查到表{_context.TableName}变更{changeData?.Count}条");
#endif
                }
                if (changeData != null && changeData.Count > 0)
                {
                    _eventBus?.Invoke(GetMessageList(changeData));
                }
                else
                {
                    Thread.Sleep(_context.Options.CdcConfig.ScanInterval);
                }
            }
        }

        /// <summary>
        /// 获取消息体
        /// </summary>
        /// <param name="changeDataList"></param>
        /// <returns></returns>
        private List<SubscribeMessage<T>> GetMessageList(List<CT<T>> changeDataList)
        {
            //订阅的消息可以忽略更新前数据
            return changeDataList.Where(x => x.Operation != CTOperationEnum.UPDATING)
                             .Select(x => new SubscribeMessage<T>
                             {
                                 Seqval = x.Seqval,
                                 Operation = x.Operation != CTOperationEnum.UPDATED ? (OperationEnum)(int)x.Operation : OperationEnum.UPDATE,
                                 Result = x.Result,
                                 TableName = _context.TableName
                             })
                             .ToList();
        }


        //代码首次进入
        private static bool _isCodeFirst = true;
        private static readonly object _checkFirstLock = new object();

        private bool _isAnalysisFirst = true;
        /// <summary>
        /// 首次进入确认
        /// </summary>
        private void CheckFirst()
        {
            if (_isAnalysisFirst)
            {
                _isAnalysisFirst = false;
                //检测CDC设置
                lock (_checkFirstLock)
                {
                    using (var connection = new SqlConnection(_context.Options.ConnectionString))
                    {
                        var database = GetDataBase();
                        bool isCdcEnabled = false;
                        if (_isCodeFirst)
                        {
                            //检查是否开启了CDC
                            isCdcEnabled = connection.QueryFirstOrDefault<bool>($"SELECT is_cdc_enabled FROM sys.databases WHERE [NAME] = '{database}'");
                            if (!isCdcEnabled)
                            {
                                //首次进入需要开启CNC
                                connection.Execute($"alter database {database} set allow_snapshot_isolation on");
                                //启动cdc监听作业
                                connection.Execute("exec sys.sp_cdc_enable_db");
                            }
                        }
                        //判断表是否开启过cdc
                        if (!IsExsitsCdc())
                        {
                            //表开启cdc
                            connection.Execute($@"exec sys.sp_cdc_enable_table
                                                @source_schema = N'{typeof(T).GetSchemaName()}', 
	                                            @source_name = N'{_context.TableName}',
	                                            @role_name = null");
                        }
                        else
                        {
                            //检查最新的变更
                            if (_context.Options.CdcConfig.OffsetPosition == OffsetPositionEnum.Abort)
                                _lastSeqval = _context.VolumeFile.ReadSeqval() ?? "0";
                            else if (_context.Options.CdcConfig.OffsetPosition == OffsetPositionEnum.Last)
                                _lastSeqval = connection.QueryFirstOrDefault<string>($"SELECT TOP 1 convert(varchar(50), [__$seqval], 1) FROM {GetCtTableName()} order by __$seqval desc") ?? "0";
                        }
                        if (_isCodeFirst)
                        {
                            _isCodeFirst = false;
                            if (!isCdcEnabled)
                            {
                                //必须在表开启后才能执行清楚计划
                                connection.Execute($@"exec sys.sp_cdc_change_job
                                                @job_type = 'cleanup',
                                                @retention = {_context.Options.CdcConfig.Retention},
                                                @threshold = 5000");
                            }
                        }
                    }
                }
            }
        }

        //当前扫描的末尾id
        private long _currLastId = 0;
        //最终扫描的末尾id
        private long _lastId = 0;
        /// <summary>
        /// 首次扫描全表
        /// </summary>
        /// <returns>是否扫描完成</returns>
        private List<CT<T>> CheckFirstScanFull()
        {
            List<CT<T>> tableList = default;
            if (_context.Options.CdcConfig.IsFirstScanFull)
            {
                using (var connection = new SqlConnection(_context.Options.ConnectionString))
                {
                    if (_currLastId == 0)
                    {
                        var lastId = connection.GetId<T>();
                        _lastId = Convert.ToInt64(lastId ?? "0");
#if DEBUG
                        Console.WriteLine($"开始扫描表{_context.TableName}的全部信息");
#endif
                    }
                    //扫描全表数据
                    var (_identityName, _identityProperty) = typeof(T).GetIdentity();
                    string whereSql = $" AND [{_identityName}] BETWEEN {_currLastId} AND {_lastId}";
                    string orderSql = $"ORDER BY {_identityName}";
                    tableList = connection.GetList<T>(_context.TableName, _context.Options.CdcConfig.Limit, whereSql, orderSql)
                        .Select(x => new CT<T>
                        {
                            Seqval = "0",
                            Operation = CTOperationEnum.INSERT,
                            Result = x
                        }).ToList();
                    if (!tableList.IsNullOrEmpty())
                    {
                        _currLastId = Convert.ToInt64(_identityProperty.GetValue(tableList!.LastOrDefault()!.Result));
                    }
                    //表示扫描完成
                    if (tableList?.Count < _context.Options.CdcConfig.Limit)
                    {
                        _context.Options.CdcConfig.IsFirstScanFull = false;
                    }
                }
            }
            return tableList;
        }



        //最后检查的seq
        private string _lastSeqval = "0";
        /// <summary>
        /// 检查变更
        /// </summary>
        private List<CT<T>> CheckChange()
        {
            using (var connection = new SqlConnection(_context.Options.ConnectionString))
            {
                //查询这批操作影响到的所有语句
                string execSql = $@"SELECT * FROM(
									    SELECT 
										    ROW_NUMBER() OVER(order by [__$seqval] asc) as [row],
	                                        convert(varchar(50), __$seqval, 1) AS [__$seqval],
	                                        [__$operation], 
                                            {string.Join(",", GetTableFields())}
                                        FROM {GetCtTableName()} 
                                        WHERE [__$seqval] > {_lastSeqval}
									    ) T
								    WHERE T.[row] BETWEEN 1 AND {_context.Options.CdcConfig.Limit}";
                var changeData = ToCT(connection.QueryDataTable(execSql));
                //记录本次seq
                if (changeData != null && changeData.Any())
                    _lastSeqval = changeData.LastOrDefault().Seqval;
                return changeData;
            }
        }

        /// <summary>
        /// 检查是否开启过CDC
        /// </summary>
        /// <returns></returns>
        private bool IsExsitsCdc()
        {
            using (var connection = new SqlConnection(_context.Options.ConnectionString))
            {
                var result = connection.QueryFirst<int>($"SELECT TOP 1 ISNULL(is_tracked_by_cdc, 0) FROM sys.tables where [name] ='{_context.TableName}'");
                return result > 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<string> GetTableFields()
        {
            return typeof(T).GetFieldInfos().Select(x => x.FieldName).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetCtTableName()
        {
            return $"cdc.{typeof(T).GetSchemaName()}_{_context.TableName}_CT";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDataBase()
        {
            using (var connection = new SqlConnection(_context.Options.ConnectionString))
            {
                return connection.Database;
            }
        }

        /// <summary>
        /// 转换成CT<T>泛型数据
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static List<CT<T>> ToCT(DataTable dataTable)
        {
            List<CT<T>> ctList = new();
            if (dataTable != null && dataTable.Rows.Count != 0)
            {
                var fields = typeof(T).GetFieldInfos();
                foreach (DataRow row in dataTable.Rows)
                {
                    CT<T> cT = new CT<T>();
                    cT.Seqval = row["__$seqval"] != null && row["__$seqval"] != DBNull.Value ? Convert.ToString(row["__$seqval"]) : default;
                    cT.Operation = (CTOperationEnum)Convert.ToInt32(row["__$operation"]);
                    //设置表变更信息
                    T result = Activator.CreateInstance<T>();
                    for (var i = 0; i < dataTable.Columns.Count; i++)
                    {
                        var column = dataTable.Columns[i];
                        var field = fields.FirstOrDefault(x => x.FieldName == column.ColumnName);
                        if (field != null)
                        {
                            var fieldValue = row[column.ColumnName];
                            if (fieldValue != null && fieldValue != DBNull.Value)
                            {
                                var propertyType = field.PropertyInfo.PropertyType;
                                //可能是可空类型
                                if (propertyType.FullName.Contains("System.Nullable") && propertyType.GenericTypeArguments != null && propertyType.GenericTypeArguments.Count() != 0)
                                    propertyType = field.PropertyInfo.PropertyType.GenericTypeArguments[0];
                                field.PropertyInfo.SetValue(result, Convert.ChangeType(fieldValue, propertyType));
                            }
                        }
                    }
                    cT.Result = result;
                    ctList.Add(cT);
                }
            }
            return ctList;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _middlewareSubscribe?.Dispose();
            _context?.Dispose();
        }
    }
}
