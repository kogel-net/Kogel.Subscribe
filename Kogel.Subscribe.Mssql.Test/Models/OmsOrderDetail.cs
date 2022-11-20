using System;
using System.Collections.Generic;
using System.Text;
using Kogel.Dapper.Extension;
using Kogel.Dapper.Extension.Attributes;
using Nest;

namespace Kogel.Subscribe.Mssql.Test.Models
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Rename = "t_oms_order_detail")]
    [ElasticsearchType(RelationName = "t_oms_order_detail", IdProperty = "Id")]
    public class OmsOrderDetail : IBaseEntity<OmsOrderDetail, int>
    {
        /// <summary>
        /// 
        /// </summary>
        [Identity]
        [Display(Rename = "id")]
        [Nest.PropertyName("id")]
        public override int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Rename = "name")]
        [Nest.PropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Rename = "trade_id")]
        [Nest.PropertyName("trade_id")]
        public int? TradeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Rename = "descption")]
        [Nest.PropertyName("descption")]
        public string Descption { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Rename = "create_time")]
        [Nest.PropertyName("create_time")]
        public DateTime CreateTime { get; set; }
    }
}