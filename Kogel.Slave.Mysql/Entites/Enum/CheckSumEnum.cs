using System;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Slave.Mysql.Entites.Enum
{
    /// <summary>
    /// binglog用来校验每个event的完整度
    /// </summary>
    public enum CheckSumEnum
    {
        /// <summary>
        /// 检查每个 event 的长度来验证写入 binlog event 的完整性
        /// </summary>
        NONE = 0,

        /// <summary>
        /// 为每个 event 写入一个校验值
        /// </summary>
        CRC32 = 4
    }
}
