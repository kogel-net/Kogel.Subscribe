using Kogel.Dapper.Extension.Attributes;
using Kogel.Subscribe.Mssql.Entites.Enum;

namespace Kogel.Subscribe.Mssql.Entites
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Schema = "cdc")]
    public class CT<T>
    {
        ///// <summary>
        ///// 同一条SQL的操作__$start_lsn会是一致
        ///// </summary>
        //[Identity(IsIncrease = false)]
        //[Display(Rename = "__$start_lsn")]
        //public override string Id { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Display(Rename = "__$end_lsn")]
        //public string EndLsn { get; set; }

        /// <summary>
        /// 变更检索的唯一标识
        /// </summary>
        [Display(Rename = "__$seqval")]
        public string Seqval { get; set; }

        /// <summary>
        /// 更新前和更新后的同一次操作Seqval会是一致
        /// </summary>
        [Display(Rename = "__$operation")]
        public CTOperationEnum Operation { get; set; }

        /// <summary>
        /// 用来存放表实体信息
        /// </summary>
        [Display(IsField = false)]
        public T Result { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Display(Rename = "__$update_mask")]
        //public string UpdateMask { get; set; }

        ///// <summary>
        ///// 
        ///// </summary>
        //[Display(Rename = "__$command_id")]
        //public long CommandId { get; set; }
    }

}
