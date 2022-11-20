using Kogel.Dapper.Extension;
using Kogel.Subscribe.Mssql.Entites;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Subscribe.Mssql.Test.Subscribe
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSubscribe<T> : Subscribe<T>
        where T : class, IBaseEntity
    {
        public override void OnConfiguring(OptionsBuilder builder)
        {
            builder.BuildConnection("server=192.168.159.128;user id=sa;password=P@ssw0rd,;persistsecurityinfo=True;database=KogelTest")
                   //builder.BuildConnection("server=172.18.44.111;user id=sa;password=p@ssw0rd;persistsecurityinfo=True;database=KogelTest")
                   //.BuilderRabbitmq(new RabbitMQ.Client.ConnectionFactory
                   //{           
                   //})
                   //.BuildKafka(new ProducerConfig
                   //{
                   //    BootstrapServers = "192.168.159.128:9092",
                   //    Acks = Acks.None
                   //})
                   .BuildElasticsearch(new Nest.ConnectionSettings(new Uri("http://192.168.159.128:9200/")))
                   .BuildCdcConfig(new CdcConfig
                   {
                       IsFirstScanFull = true
                   })
                   ;
        }

        /// <summary>
        /// 订阅变更 （每一次sql的执行会触发一次Subscribe）
        /// </summary>
        /// <param name="messageList">消息列表表示所有影响到的数据变更(会受BuildLimit限制，没有查询完成的会在下一次查出)</param>
        public override void Subscribes(List<SubscribeMessage<T>> messageList)
        {
            foreach (var message in messageList)
            {
                Console.WriteLine($"执行动作:{message.Operation}，更新的id:{message.Result.GetId()}");
            }
        }
    }
}
