using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kogel.Slave.Mysql
{
    interface ILogEventFactory
    {
        LogEvent Create(object context);
    }
}
