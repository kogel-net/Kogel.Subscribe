using System;
using System.Collections.Generic;

namespace Kogel.Slave.Mysql
{
    public class DefaultCharset
    {
        public int DefaultCharsetCollation { get; set; }

        public Dictionary<int, int> CharsetCollations { get; set; }
    }
}
