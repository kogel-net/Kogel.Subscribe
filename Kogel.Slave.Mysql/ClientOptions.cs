using MySql.Data.MySqlClient;

namespace Kogel.Slave.Mysql
{
    public class ClientOptions
    {
        /// <summary>
        /// 连接HOST
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        public int Port { get; set; } = 3306;

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 如果为空时，自动设置一个可用的id
        /// </summary>
        public int? ServerId { get; set; }

        /// <summary>
        /// Mysql版本
        /// </summary>
        public Version? Version { get; set; }

        public string GetConnectionString()
        {
            return $"Server={Server};PORT={Port}; UID={UserName}; Password={Password}";
        }

        public MySqlConnection GetConnection()
        {
            string connString = GetConnectionString();
            return new MySqlConnection(connString);
        }
    }
}
