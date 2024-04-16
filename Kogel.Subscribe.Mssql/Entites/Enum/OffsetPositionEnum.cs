namespace Kogel.Subscribe.Mssql.Entites.Enum
{
    /// <summary>
    /// 偏移量位置
    /// </summary>
    public enum OffsetPositionEnum
    {
        /// <summary>
        /// 最早
        /// </summary>
        Zero = 0,

        /// <summary>
        /// 最后中止处
        /// </summary>
        Abort = 1,

        /// <summary>
        /// 末尾，最新产生的cdc
        /// </summary>
        Last = 2,
    }
}
