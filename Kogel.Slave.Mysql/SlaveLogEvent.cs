using Kogel.Slave.Mysql.Entites.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Slave.Mysql
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SlaveLogEvent
    {
        /// <summary>
        /// 校验方式
        /// </summary>
        public static CheckSumEnum BinLogCheckSum { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public LogEventTypeEnum LogEventType { get; set; }

        /// <summary>
        /// 从节点id（需要保持唯一）
        /// </summary>
        public int ServerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LogSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LogPosition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LogEventFlagEnum LogEventFlag { get; set; }
    }
}
