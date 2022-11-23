using Kogel.Dapper.Extension.Attributes;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using Kogel.Subscribe.Mssql.Entites;
using System.Linq;
using Confluent.Kafka;
using System.Diagnostics.Contracts;
using Kogel.Subscribe.Mssql.Test.Models;
using Kogel.Dapper.Extension.MsSql.Extension;
using Kogel.Dapper.Extension;
using Kogel.Subscribe.Mssql.Entites.Enum;
using System.Threading;

namespace Kogel.Subscribe.Mssql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //EntityCache.Register(typeof(OmsOrderDetail));
            //var codeFirst = new CodeFirst(new SqlConnection("server=192.168.159.128;user id=sa;password=P@ssw0rd,;persistsecurityinfo=True;database=KogelTest"));
            //codeFirst.SyncStructure();

            SubscribeProgram.Run();

            Console.ReadLine();
        }
    }
}