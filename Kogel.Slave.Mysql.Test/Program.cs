using System;
using Kogel.Dapper.Extension.MySql;
using Kogel.Dapper.Extension;
using MySql.Data.MySqlClient;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using System.IO;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Kogel.Slave.Mysql;
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
                UserName = "root",
                Password = "123456"
            };

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

   
}
