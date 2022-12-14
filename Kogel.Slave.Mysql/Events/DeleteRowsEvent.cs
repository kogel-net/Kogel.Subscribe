using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SuperSocket.ProtoBase;

namespace Kogel.Slave.Mysql
{
    public sealed class DeleteRowsEvent :  RowsEvent
    {
        public DeleteRowsEvent()
            : base()
        {

        }
    }
}
