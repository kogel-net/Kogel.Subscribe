using Kogel.Subscribe.Mssql.Entites;
using Kogel.Subscribe.Mssql.Entites.Enum;
using Kogel.Subscribe.Mssql.Test.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Subscribe.Mssql.Test.Subscribe
{
    /// <summary>
    /// 
    /// </summary>
    public class OrderSubscribe : BaseSubscribe<Orders>
    {
        public override void OnConfiguring(OptionsBuilder<Orders> builder)
        {
            base.OnConfiguring(builder);
            //配置所有表分片
            builder.BuildShards(new List<string>
            {
                "orders_1",
                //"orders_2",
                //"orders_3"
            });

            builder.BuildElasticsearch(new ElasticsearchConfig<Orders>
            {
                Settings = new Nest.ConnectionSettings(new Uri("http://192.168.159.128:9200/")),
                WriteInterceptor = x => WriteInterceptor(x)
            });

            builder.BuildCdcConfig(new CdcConfig
            {
                OffsetPosition = OffsetPositionEnum.Abort,
                IsFirstScanFull = true
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private EsSubscribeMessage<object> WriteInterceptor(SubscribeMessage<Orders> message)
        {
            string esIndexName;
            //这里写自己索引分片的业务逻辑
            if (message.Result.Id % 3 == 0)
            {
                esIndexName = $"kogel_orders_2";
            }
            else
            {
                esIndexName = $"kogel_orders_1";
            }
            return message.ToEsSubscribeMessage(esIndexName, message.Result);
        }
    }
}
