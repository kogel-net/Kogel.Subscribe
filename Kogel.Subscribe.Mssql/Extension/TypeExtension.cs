namespace Kogel.Subscribe.Mssql.Extension
{
    public static class TypeExtension
    {
        public static string GetTableName(this Type entityType)
        {
            string tableName = entityType.Name;
            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            return tableAttr != null && !string.IsNullOrEmpty(tableAttr.Name) ? tableAttr.Name : tableName;
        }

        public static string GetSchemaName(this Type entityType)
        {
            string schemaName = entityType.Name;
            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            return tableAttr != null && !string.IsNullOrEmpty(tableAttr.Schema) ? tableAttr.Schema : schemaName;
        }

        /// <summary>
        /// 获取主键属性
        /// </summary>
        /// <returns></returns>
        public static (string, PropertyInfo) GetIdentity(this Type entityType)
        {
            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                var keyAttr = property.GetCustomAttribute<KeyAttribute>();
                if (keyAttr != null)
                {
                    return (property.Name, property);
                }
                else
                {
                    if (property.Name.ToLower().Equals("id"))
                    {
                        return (property.Name, property);
                    }
                }
            }
            throw new NotImplementedException("没有查找到对应的主键");
        }

        public static List<FieldInfo> GetFieldInfos(this Type entityType)
        {
            List<FieldInfo> fields = new();
            var properties = entityType.GetProperties();
            foreach (var property in properties)
            {
                string fieldName = property.Name;
                var columnAttr = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttr != null && !string.IsNullOrEmpty(columnAttr.Name))
                {
                    fieldName = columnAttr.Name;
                }
                fields.Add(new FieldInfo
                {
                    PropertyName = property.Name,
                    FieldName = fieldName,
                    PropertyInfo = property,
                });
            }
            return fields;
        }
    }

    public class FieldInfo
    {
        public string PropertyName { get; set; }

        public string FieldName { get; set; }

        public PropertyInfo PropertyInfo { get; set; }
    }


}
