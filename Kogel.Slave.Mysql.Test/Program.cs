using System;
using Kogel.Dapper.Extension.MySql;
using Kogel.Dapper.Extension;
using MySql.Data.MySqlClient;

namespace Kogel.Slave.Mysql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            using (var conn = new MySqlConnection("server=120.25.103.184;port=3306;user id=root;password=123456.;persistsecurityinfo=True;database=yuefeng_logistics_dev;SslMode=none;allowzerodatetime=false;"))
            {
                conn.Open();
            }
        }
    }
}
