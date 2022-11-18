using Kogel.Dapper.Extension.Attributes;
using System;
using System.Data.SqlClient;
using Kogel.Repository;
using System.Collections.Generic;
using Kogel.Subscribe.Mssql.Entites;
using System.Linq;
using Confluent.Kafka;
using System.Diagnostics.Contracts;
using Kogel.Subscribe.Mssql.Test.Models;

namespace Kogel.Subscribe.Mssql.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            //OrderDetailDataSyncEs dataSyncEs = new OrderDetailDataSyncEs();

            Kogel.Subscribe.Mssql.Program.Run();

            Console.ReadLine();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class OrderDetailDataSyncEs : Subscribe<OmsOrderDetail>
    {
        public override void OnConfiguring(OptionsBuilder builder)
        {
            builder.BuildConnection("server=172.18.44.111;user id=sa;password=p@ssw0rd;persistsecurityinfo=True;database=KogelTest")
                .BuildLimit(100)
                   //.BuildFirstScanFull(true)
                   //.BuilderRabbitmq(new RabbitMQ.Client.ConnectionFactory
                   //{           
                   //})
                   //.BuildKafka(new Confluent.Kafka.ProducerConfig {
                   //    BootstrapServers = "localhost:9092",
                   //    Acks = Acks.All
                   //})
                   //.BuildTopic("kogel_subscribe_t_oms_order_detail")
                   ;
        }

        /// <summary>
        /// 订阅变更 （每一次sql的执行会触发一次Subscribe）
        /// </summary>
        /// <param name="messageList">消息列表表示所有影响到的数据变更(会受BuildLimit限制，没有查询完成的会在下一次查出)</param>
        public override void Subscribes(List<SubscribeMessage<OmsOrderDetail>> messageList)
        {
            foreach (var message in messageList)
            {
                Console.WriteLine($"执行动作:{message.Operation}，更新的id:{message.Result.Id}");
            }
        }
    }

}
