using System;
using Kogel.Dapper.Extension.MySql;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using SuperSocket.Client;

namespace Kogel.Slave.Mysql.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var client = new SlaveClient();
            var options = new ClientOptions
            {
                Server = "127.0.0.1",
                Port = 3307,
                UserName = "root",
                Password = "123456",
            };

            var connString = $"Server={options.Server};PORT={options.Port}; UID={options.UserName}; Password={options.Password};Database=kogel_test;";
            var conn = new MySqlConnection(connString);

            client.PackageHandler += Client_PackageHandler;

            var result = await client.ConnectAsync(options);

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


            await Task.CompletedTask;
        }







    }

    public class usr_student
    {
        public int id { get; set; }

        public string code { get; set; }

        public string name { get; set; }

        public DateTime create_time { get; set; }
    }


}
