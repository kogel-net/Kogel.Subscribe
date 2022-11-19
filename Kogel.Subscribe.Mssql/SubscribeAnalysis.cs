using Kogel.Subscribe.Mssql.Entites;
using System;
using System.Collections.Generic;
using System.Threading;
using Kogel.Repository;
using System.Data.SqlClient;
using Kogel.Dapper.Extension.MsSql;
using Kogel.Repository.Interfaces;
using System.Linq;
using System.Data.Common;
using System.Reflection;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Kogel.Subscribe.Mssql.Middleware;
using Kogel.Dapper.Extension;
using System.Data;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 解析
    /// </summary>
    public sealed class SubscribeAnalysis<T> : IDisposable
        where T : class
    {
        /// <summary>
        /// 订阅通知
        /// </summary>
        /// <param name="messageList"></param>
        public delegate void SubscribeHandler(List<SubscribeMessage<T>> messageList);

        /// <summary>
        /// 事件总线
        /// </summary>
        public event SubscribeHandler EventBus;

        /// <summary>
        /// 
        /// </summary>
        private OptionsBuilder _options;

        /// <summary>
        /// 
        /// </summary>
        private MiddlewareSubscribe<T> _middlewareSubscribe;

        /// <summary>
        /// 
        /// </summary>
        private IBaseRepository<T> _repoistory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal SubscribeAnalysis<T> Register(ISubscribe<T> handler, OptionsBuilder options)
        {
            this._options = options;
            EventBus += new SubscribeHandler(handler.Subscribes);

            //注册中间件
            if (options.MiddlewareTypeList.Any())
            {
                _middlewareSubscribe = new MiddlewareSubscribe<T>(options).Register();
                EventBus += new SubscribeHandler(_middlewareSubscribe.Subscribes);
            }

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
                    Console.WriteLine($"开始检查表{GetTableName()}变更");
#endif
                    //检查变更
                    changeData = CheckChange();
#if DEBUG
                    Console.WriteLine($"检查到表{GetTableName()}变更{changeData?.Count}条");
#endif
                }
                if (changeData != null && changeData.Count > 0)
                {
                    EventBus?.Invoke(GetMessageList(changeData));
                }
                else
                {
                    Thread.Sleep(_options.CdcConfig.ScanInterval);
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
                                 Result = x.Result
                             })
                             .ToList();
        }


        //代码首次进入
        private static bool _isCodeFirst = true;
        private static readonly object _checkFirstLock = new object();

        /// <summary>
        /// 首次进入确认
        /// </summary>
        private void CheckFirst()
        {
            if (_repoistory is null)
            {
                RepositoryOptionsBuilder.RegisterDataBase(x => new SqlConnection(_options.ConnectionString), "master");
                _repoistory = new SqlConnection(_options.ConnectionString).QuerySet<T>().GetRepository();
                //检测CDC设置
                lock (_checkFirstLock)
                {
                    var database = GetDataBase();
                    bool isCdcEnabled = false;
                    if (_isCodeFirst)
                    {
                        //检查是否开启了CDC
                        isCdcEnabled = _repoistory.Orm.QueryFirstOrDefault<bool>($"SELECT is_cdc_enabled FROM sys.databases WHERE [NAME] = '{database}'");
                        if (!isCdcEnabled)
                        {
                            //首次进入需要开启CNC
                            _repoistory.Orm.Execute($"alter database {database} set allow_snapshot_isolation on");
                            //启动cdc监听作业
                            _repoistory.Orm.Execute("exec sys.sp_cdc_enable_db");
                        }
                    }
                    //判断表是否开启过cdc
                    if (!IsExsitsCdc())
                    {
                        //表开启cdc
                        _repoistory.Orm.Execute($@"exec sys.sp_cdc_enable_table
                                                @source_schema = N'{GetSchema()}', 
	                                            @source_name = N'{GetTableName()}',
	                                            @role_name = null");
                    }
                    else
                    {
                        //检查最新的变更
                        if (_options.CdcConfig.OffsetPosition == OffsetPositionEnum.Last)
                            _lastSeqval = _repoistory.Orm.QueryFirstOrDefault<string>($"SELECT TOP 1 convert(varchar(50), [__$seqval], 1) FROM {GetCtTableName()} order by __$seqval desc") ?? "0";
                    }
                    if (_isCodeFirst)
                    {
                        _isCodeFirst = false;
                        if (!isCdcEnabled)
                        {
                            //必须在表开启后才能执行清楚计划
                            _repoistory.Orm.Execute($@"exec sys.sp_cdc_change_job
                                                @job_type = 'cleanup',
                                                @retention = {_options.CdcConfig.Retention},
                                                @threshold = 5000");
                        }
                    }
                }
            }
        }

        //末尾id
        private long _lastId = 0;
        /// <summary>
        /// 首次扫描全表
        /// </summary>
        /// <returns>是否扫描完成</returns>
        private List<CT<T>> CheckFirstScanFull()
        {
            List<CT<T>> tableList = default;
            if (_options.CdcConfig.IsFirstScanFull)
            {
#if DEBUG
                if (_lastId == 0)
                    Console.WriteLine($"开始扫描表{GetTableName()}的全部信息");
#endif
                //获取主键属性
                var (idName, idProperty) = GetIdentity();
                //扫描全表数据
                tableList = _repoistory.QuerySet()
                      .Where($"[{idName}] > {_lastId}")
                      .OrderBy($" {idName} ASC ")
                      .Page(1, _options.CdcConfig.Limit)
                      .Select(x => new CT<T>
                      {
                          Seqval = "0",
                          Operation = CTOperationEnum.INSERT,
                          Result = x
                      }).ToList();
                if (tableList.Any())
                {
                    _lastId = Convert.ToInt64(idProperty.GetValue(tableList.LastOrDefault().Result));
                }
                //表示扫描完成
                if (tableList.Count < _options.CdcConfig.Limit)
                {
                    _options.CdcConfig.IsFirstScanFull = false;
                }
            }
            return tableList;
        }

        //主键名称
        private string _identityName;
        //主键反射对象
        private PropertyInfo _identityProperty;
        /// <summary>
        /// 获取主键属性
        /// </summary>
        /// <returns></returns>
        private (string, PropertyInfo) GetIdentity()
        {
            if (string.IsNullOrEmpty(_identityName))
            {
                var entity = EntityCache.QueryEntity(typeof(T));
                try
                {
                    _identityName = entity.Identitys;
                    _identityProperty = entity.EntityFieldList.FirstOrDefault(x => x.FieldName.Contains(_identityName)).PropertyInfo;
                }
                catch
                {
                    throw new Exception($"请用特性[Identity]标记表{entity.Name}的主键特性!");
                }
            }
            return (_identityName, _identityProperty);
        }

        //最后检查的seq
        private string _lastSeqval = "0";
        /// <summary>
        /// 检查变更
        /// </summary>
        private List<CT<T>> CheckChange()
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
								    WHERE T.[row] BETWEEN 1 AND {_options.CdcConfig.Limit}";
            var changeData = ToCT(_repoistory.Orm.QueryDataSet(new SqlDataAdapter(), execSql));
            //记录本次seq
            if (changeData != null && changeData.Any())
                _lastSeqval = changeData.LastOrDefault().Seqval;
            return changeData;
        }

        /// <summary>
        /// 检查是否开启过CDC
        /// </summary>
        /// <returns></returns>
        private bool IsExsitsCdc()
        {
            var result = _repoistory.Orm.QueryFirst<int>($"SELECT TOP 1 is_tracked_by_cdc FROM sys.tables where [name] ='{GetTableName()}'");
            return result > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private List<string> GetTableFields()
        {
            return EntityCache.QueryEntity(typeof(T)).EntityFieldList.Select(x => x.FieldName).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetTableName()
        {
            var entity = EntityCache.QueryEntity(typeof(T));
            return entity.Name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetCtTableName()
        {
            return $"cdc.{GetSchema()}_{GetTableName()}_CT";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetSchema()
        {
            var entity = EntityCache.QueryEntity(typeof(T));
            return !string.IsNullOrEmpty(entity.Schema) ? entity.Schema : "dbo";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDataBase()
        {
            var dbConnection = _repoistory.Orm as DbConnection;
            return dbConnection?.Database;
        }

        /// <summary>
        /// 转换成CT<T>泛型数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static List<CT<T>> ToCT(DataSet dataSet)
        {
            List<CT<T>> ctList = new List<CT<T>>();
            var entity = EntityCache.QueryEntity(typeof(T));
            if (dataSet != null && dataSet.Tables.Count != 0 && dataSet.Tables[0].Rows.Count != 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    CT<T> cT = new CT<T>();
                    cT.Seqval = row["__$seqval"] != null && row["__$seqval"] != DBNull.Value ? Convert.ToString(row["__$seqval"]) : default;
                    cT.Operation = (CTOperationEnum)Convert.ToInt32(row["__$operation"]);
                    //设置表变更信息
                    T result = Activator.CreateInstance<T>();
                    for (var i = 0; i < dataSet.Tables[0].Columns.Count; i++)
                    {
                        var column = dataSet.Tables[0].Columns[i];
                        var field = entity.EntityFieldList.FirstOrDefault(x => x.FieldName == column.ColumnName);
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
            _repoistory?.Dispose();
            _middlewareSubscribe?.Dispose();
        }
    }
}
