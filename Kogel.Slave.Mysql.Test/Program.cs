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

namespace Kogel.Slave.Mysql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //test connect
            using (var conn = new MySqlConnection("server=192.168.159.128;port=3306;user id=root;password=123456;persistsecurityinfo=True;database=KogelTest;SslMode=none;allowzerodatetime=false;"))
            {
                conn.Open();
            }

      

        }

     
   

      




    }

   
}
