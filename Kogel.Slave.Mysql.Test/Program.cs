using System;
using Kogel.Dapper.Extension.MySql;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using SuperSocket.Client;
using Kogel.Dapper.Extension.Core.SetQ;
using System.Text.Json;
using System.Text;

namespace Kogel.Slave.Mysql.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var options = new ClientOptions
            {
                Server = "127.0.0.1",
                Port = 3306,
                UserName = "root",
                Password = "123456"
            };

            var client = new SlaveClient(options);

            client.PackageHandler += Client_PackageHandler;
            var result = await client.ConnectAsync();

            if (!result.Result)
            {
                Console.WriteLine($"Failed to connect: {result.Message}.");
                return;
            }

            Console.ReadLine();
            await client.CloseAsync();
        }

        private static async ValueTask Client_PackageHandler(EasyClient<LogEvent> sender, LogEvent package)
        {
            Type eventType = package.GetType();

            //更新
            if (eventType.Equals(typeof(UpdateRowsEvent)))
            {
                 
            }
            //新增
            else if (eventType.Equals(typeof(RowsEvent)))
            {

            }

            await Task.CompletedTask;
        }
    }
}
