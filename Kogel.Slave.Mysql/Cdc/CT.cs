using Kogel.Dapper.Extension.Attributes;

namespace Kogel.Slave.Mysql.Cdc
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Rename = "cdc_ct")]
    public class CT
    {
        /// <summary>
        /// 变更检索的唯一标识
        /// </summary>
        [Display(Rename = "__$seqval")]
        [Identity]
        public string Seqval { get; set; }

        /// <summary>
        /// 更新前和更新后的同一次操作Seqval会是一致
        /// </summary>
        [Display(Rename = "__$operation")]
        public CTOperationEnum Operation { get; set; }

        /// <summary>
        /// 变更的内容
        /// </summary>
        [Display(Rename = "__$result")]
        public string Result { get; set; }
    }
}
