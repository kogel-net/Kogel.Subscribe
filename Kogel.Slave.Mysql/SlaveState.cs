using System.Collections.Generic;

namespace Kogel.Slave.Mysql
{
    class SlaveState
    {

        public Dictionary<long, TableMapEvent> TableMap { get; set; } = new Dictionary<long, TableMapEvent>();
    }
}
