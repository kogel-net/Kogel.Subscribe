using static Dapper.SqlMapper;

namespace Kogel.Subscribe.Mssql.Extension
{
    public static class DapperExtension
    {
        public static string? GetId<T>(this IDbConnection connection)
        {
            Type tableType = typeof(T);
            var (_identityName, _identityProperty) = tableType.GetIdentity();
            string tableName = tableType.GetTableName();
            string querySql = $"SELECT TOP 1 {_identityName} FROM {tableName} ORDER BY {_identityName} DESC";
            return connection.QueryFirstOrDefault<string>(querySql);
        }

        public static List<T> GetList<T>(this IDbConnection connection, string? tableName = null, int pageSize = 10, string? whereSql = null, string? orderSql = null)
        {
            Type tableType = typeof(T);
            tableName ??= tableType.GetTableName();
            var fields = tableType.GetFieldInfos();
            string querySql = $@"
SELECT TOP {pageSize} {string.Join(",", fields.Select(x => x.FieldName))} FROM {tableName}
WHERE 1 = 1 {whereSql}
{orderSql}";
            return connection.Query<T>(querySql).ToList();
        }

        public static DataTable QueryDataTable(this IDbConnection connection, string sql, object? param = null, IDbTransaction transaction = null)
        {
            DataTable table = new();
            var reader = connection.ExecuteReader(sql, param, transaction);
            table.Load(reader);
            return table;
        }
    }
}
