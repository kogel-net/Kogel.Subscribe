using System;
using System.Collections.Generic;
using System.Text;

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
        /// 最后/最新/当前
        /// </summary>
        Last = 1,
    }
}
