using Kogel.Subscribe.Mssql.Entites.Enum;

namespace Kogel.Subscribe.Mssql.Entites
{
    /// <summary>
    /// 
    /// </summary>
    public class CdcConfig
    {
        /// <summary>
        /// 扫描间隔(每次扫描变更表的间隔，单位毫秒) 默认10000毫秒/10秒
        /// </summary>
        public int ScanInterval { get; set; } = 10000;

        /// <summary>
        /// 变更捕捉文件在DB保存的时间（默认三天）
        /// </summary>
        public int Retention { get; set; } = 60 * 24 * 3;

        /// <summary>
        /// 是否首次扫描表全部数据在监听变更
        /// </summary>
        public bool IsFirstScanFull { get; set; } = false;

        /// <summary>
        /// 每次检索的变更量
        /// </summary>
        public int Limit { get; set; } = 10;

        /// <summary>
        /// 扫描的偏移量位置（默认从起点开始）
        /// </summary>
        public OffsetPositionEnum OffsetPosition { get; set; } = OffsetPositionEnum.Abort;

        ///// <summary>
        ///// 是否异常中断（订阅消费过程中发生异常是否会影响后续增量捕捉，默认会影响）
        ///// </summary>
        //internal bool IsErrorInterrupt { get; set; } = true;
    }
}
