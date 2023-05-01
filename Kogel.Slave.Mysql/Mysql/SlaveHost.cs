namespace Kogel.Slave.Mysql.Mysql
{
    public class SlaveHost
    {
        public string Server_id { get; set; }

        public string Host { get; set; }

        public string Port { get; set; }

        public string Master_id { get; set; }

        public string Slave_UUID { get; set; }
    }
}
