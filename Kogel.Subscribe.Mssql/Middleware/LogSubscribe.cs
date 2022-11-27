using Kogel.Subscribe.Mssql.Entites;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using static Kogel.Subscribe.Mssql.Middleware.FileExtension;
using System.IO;

namespace Kogel.Subscribe.Mssql.Middleware
{
    /// <summary>
    /// 日志订阅（记录每次执行的Seqval到本地）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LogSubscribe<T> : ISubscribe<T>
        where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly SubscribeContext<T> _context;

        /// <summary>
        /// 
        /// </summary>
        private readonly FileObjectLock _fileObjectLock;
        public LogSubscribe(SubscribeContext<T> context)
        {
            this._context = context;
            this._fileObjectLock = FileObjectLock.CreateOrGet(context._tableName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageList"></param>
        public void Subscribes(List<SubscribeMessage<T>> messageList)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class FileExtension
    {
        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static string GetPath(string tableName) => $"{Directory.GetCurrentDirectory()}//{DateTime.Now.ToString("yyyyMMdd")}//{tableName}.txt";

        /// <summary>
        /// 初始化路径
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        private static void InitWrite(string path)
        {
            string directoryFullName = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryFullName))
            {
                Directory.CreateDirectory(directoryFullName);
            }
            if (!File.Exists(path))
            {
                File.CreateText(path).Dispose();
            }
        }

        ///// <summary>
        ///// 读取seq
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="context"></param>
        ///// <returns></returns>
        //public static string ReadSeqval<T>(this SubscribeContext<T> context)
        //{
        //    string path = GetPath(context._tableName);
        //    InitPath(path);
        //    if ()
        //    {

        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="seqval"></param>
        public static void WriterSeqval<T>(this SubscribeContext<T> context, string seqval)
        {
        
        }

        /// <summary>
        /// 文件对象锁
        /// </summary>
        public sealed class FileObjectLock : object
        {
            private static readonly ThreadLocal<FileObjectLock> _local;

            private readonly string _path;

            static FileObjectLock()
            {
                _local = new ThreadLocal<FileObjectLock>();
            }

            private FileObjectLock(string path)
            {
                this._path = path;
            }

            /// <summary>
            /// 创建或者得到一个文件锁
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static FileObjectLock CreateOrGet(string path)
            {
                lock (_local)
                {
                    var lockLocal = _local.Values?.FirstOrDefault(x => x._path == path);
                    if (lockLocal is null)
                    {
                        _local.Value = new FileObjectLock(path);
                        lockLocal = _local.Value;
                    }
                    return lockLocal;
                }
            }
        }
    }
}
