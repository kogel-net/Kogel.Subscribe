using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kogel.Slave.Mysql
{
    public enum ChecksumType : int
    {
        NONE = 0,
        CRC32 = 4
    }
}
