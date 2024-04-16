namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 对象锁
    /// </summary>
    public sealed class ObjectLock : object
    {
        private static readonly ThreadLocal<ObjectLock> _local = new(true);
        private readonly string _key;
        private ObjectLock(string key)
        {
            this._key = key;
        }

        /// <summary>
        /// 创建或者得到一个文件锁
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ObjectLock CreateOrGet(string lockKey)
        {
            lock (_local)
            {
                var lockLocal = _local.Values?.FirstOrDefault(x => x._key == lockKey);
                if (lockLocal is null)
                {
                    _local.Value = new ObjectLock(lockKey);
                    lockLocal = _local.Value;
                }
                return lockLocal;
            }
        }
    }
}
