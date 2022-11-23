using Kogel.Subscribe.Mssql.Entites.Enum;

namespace Kogel.Subscribe.Mssql.Entites
{
    /// <summary>
    /// 订阅消息体
    /// </summary>
    public class SubscribeMessage<T>
    {
        /// <summary>
        /// 
        /// </summary>
        public string Seqval { get; set; }

        /// <summary>
        /// 更新前和更新后的同一次操作Seqval会是一致
        /// </summary>
        public OperationEnum Operation { get; set; }

        /// <summary>
        /// 用来存放表实体信息
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="esIndexName"></param>
        /// <returns></returns>
        public EsSubscribeMessage<T> ToEsSubscribeMessage(string esIndexName)
        {
            return new EsSubscribeMessage<T>
            {
                Seqval = this.Seqval,
                Operation = this.Operation,
                Result = this.Result,
                TableName = this.TableName,
                EsIndexName = esIndexName
            };
        }
    }
}
