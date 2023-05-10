using System;

namespace Kogel.Slave.Mysql
{
    [Flags]
    public enum RowsEventFlags : byte
    {
        EndOfStatement = 0x01,
        NoForeignKeyChecks = 0x02,
        NoUniqueKeyChecks = 0x04,
        RowHasAColumns = 0x08    
    }
}
