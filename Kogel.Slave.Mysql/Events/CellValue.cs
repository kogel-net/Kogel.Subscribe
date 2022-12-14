using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ProtoBase;

namespace Kogel.Slave.Mysql
{
    public class CellValue
    {
        public object OldValue { get; set; }

        public object NewValue { get; set; }
    }
}