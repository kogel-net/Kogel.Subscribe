namespace Kogel.Subscribe.Mssql.Entites
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EsSubscribeMessage<T> : SubscribeMessage<T>
    {
        /// <summary>
        /// 索引名称
        /// </summary>
        public string EsIndexName { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class EsSubscribeMessageExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="esIndexName"></param>
        /// <returns></returns>
        public static EsSubscribeMessage<object> ToEsSubscribeMessage<T>(this SubscribeMessage<T> message, string esIndexName)
        {
            return new EsSubscribeMessage<object>
            {
                Seqval = message.Seqval,
                Operation = message.Operation,
                Result = message.Result,
                TableName = message.TableName,
                EsIndexName = esIndexName
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="esIndexName"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static EsSubscribeMessage<object> ToEsSubscribeMessage<T>(this SubscribeMessage<T> message, string esIndexName, object result)
        {
            var esMessage = message.ToEsSubscribeMessage(esIndexName);
            esMessage.Result = result;
            return esMessage;
        }
    }
}
