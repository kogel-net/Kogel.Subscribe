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

namespace Kogel.Slave.Mysql.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //test connect
            using (var conn = new MySqlConnection("server=192.168.159.128;port=3306;user id=root;password=123456;persistsecurityinfo=True;database=KogelTest;SslMode=none;allowzerodatetime=false;"))
            {
                conn.Open();
            }

            var serverHost = "192.168.159.128";
            var username = "root";
            var password = "123456";
            var serverId = 123456; // replication server id

            var client = new SlaveClient();


            client.PackageHandler += Client_PackageHandler;

            var result = await client.ConnectAsync(serverHost, username, password, serverId);


            if (!result.Result)
            {
                Console.WriteLine($"Failed to connect: {result.Message}.");
                return;
            }

    
            Console.ReadLine();

            await client.CloseAsync();

        }

        private static async ValueTask Client_PackageHandler(SuperSocket.Client.EasyClient<LogEvent> sender, LogEvent package)
        {
          

            await Task.CompletedTask;
        }







    }

   
}
