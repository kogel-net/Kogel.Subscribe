using Kogel.Subscribe.Mssql.Entites.Enum;

namespace Kogel.Subscribe.Mssql.Entites
{
    /// <summary>
    /// 订阅消息体
    /// </summary>
    public class SubscribeMessage<T>
    {
        /// <summary>
        /// 变更序列号
        /// </summary>
        public string Seqval { get; internal set; }

        /// <summary>
        /// 操作动作
        /// </summary>
        public OperationEnum Operation { get; internal set; }

        /// <summary>
        /// 用来存放表实体信息
        /// </summary>
        public T Result { get; internal set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; internal set; }
    }
}
