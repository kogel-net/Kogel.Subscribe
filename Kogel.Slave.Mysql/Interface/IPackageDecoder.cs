using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Slave.Mysql.Interface
{
    /// <summary>
    /// 包解码
    /// </summary>
    /// <typeparam name="TPackageInfo"></typeparam>
    public interface IPackageDecoder<out TPackageInfo>
    {
        TPackageInfo Decode(ref ReadOnlySequence<byte> buffer, object context);
    }
}
