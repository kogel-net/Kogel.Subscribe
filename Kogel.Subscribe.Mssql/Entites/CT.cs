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
    }

}
