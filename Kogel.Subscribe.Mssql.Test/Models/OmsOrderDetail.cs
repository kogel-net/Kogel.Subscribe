using System;
using System.Collections.Generic;
using System.Text;
using Kogel.Dapper.Extension.Attributes;

namespace Kogel.Subscribe.Mssql.Test.Models
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Rename = "t_oms_order_detail")]
    public class OmsOrderDetail
    {
        /// <summary>
        /// 
        /// </summary>
        [Identity]
        [Display(Rename = "id")]
        public int Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Rename = "name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Rename = "trade_id")]
        public int? TradeId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Rename = "descption")]
        public string Descption { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Display(Rename = "create_time")]
        public DateTime CreateTime { get; set; }
    }
}
