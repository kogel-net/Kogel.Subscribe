using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Kogel.Slave.Mysql.Extension
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReadOnlySequenceExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetString(in this ReadOnlySequence<byte> payload, Encoding encoding = null)
        {
            if (encoding is null)
                encoding = Encoding.UTF8;
            if (payload.IsSingleSegment)
            {
                byte[] bytes = new byte[payload.First.Span.Length];
                payload.First.Span.CopyTo(bytes);
                return encoding.GetString(bytes);
            }
            else
            {
                return GetStringSlow(payload, encoding);
            }
        }
        static string GetStringSlow(in ReadOnlySequence<byte> payload, Encoding encoding)
        {
            // linearize
            int length = checked((int)payload.Length);
            var oversized = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                payload.CopyTo(oversized);
                return encoding.GetString(oversized, 0, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(oversized);
            }
        }
    }
}
