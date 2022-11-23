using Kogel.Dapper.Extension;
using Kogel.Dapper.Extension.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Subscribe.Mssql.Test.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Orders : IBaseEntity<Orders, int>
    {
        [Identity(IsIncrease = false)]
        [Display(Rename = "id")]
        public override int Id { get; set; }

        public string order_no { get; set; }

        public string content { get; set; }

        public decimal? price { get; set; }

        public DateTime create_time { get; set; }
    }
}
