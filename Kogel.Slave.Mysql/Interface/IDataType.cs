using Kogel.Slave.Mysql.Extension;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Slave.Mysql.Interface
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public interface IDataType
    {
        object ReadValue(ref SequenceReader<byte> reader, int meta);
    }
}
