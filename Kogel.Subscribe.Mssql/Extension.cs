using Kogel.Subscribe.Mssql.Entites;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Kogel.Dapper.Extension;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static List<CT<T>> ConvertCTData<T>(this DataSet dataSet)
            where T : class
        {
            List<CT<T>> ctList = new List<CT<T>>();
            var entity = EntityCache.QueryEntity(typeof(T));
            if (dataSet != null && dataSet.Tables.Count != 0 && dataSet.Tables[0].Rows.Count != 0)
            {
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    CT<T> cT = new CT<T>();
                    //cT.Id = row["__$seqval"].ToString();
                    //cT.EndLsn = row["__$end_lsn"] != null && row["__$end_lsn"] != DBNull.Value ? Convert.ToString(row["__$end_lsn"]) : default;
                    cT.Seqval = row["__$seqval"] != null && row["__$seqval"] != DBNull.Value ? Convert.ToString(row["__$seqval"]) : default;
                    cT.Operation = (CTOperationEnum)Convert.ToInt32(row["__$operation"]);
                    //cT.UpdateMask = row["__$update_mask"]?.ToString();
                    //cT.CommandId = row["__$command_id"] != null && row["__$command_id"] != DBNull.Value ? Convert.ToInt64(row["__$command_id"]) : default;
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
                                {
                                    propertyType = field.PropertyInfo.PropertyType.GenericTypeArguments[0];
                                }
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
        /// 比对验证开放的泛型
        /// </summary>
        /// <param name="givenType"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                    return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            Type baseType = givenType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericType);
        }
    }
}
