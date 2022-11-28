using Kogel.Subscribe.Mssql.Middleware;
using System;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 上下文对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SubscribeContext<T> : IDisposable
    {
        /// <summary>
        /// 配置
        /// </summary>
        public OptionsBuilder<T> Options { get; }

        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// 持久化文件
        /// </summary>
        public VolumeFile<T> VolumeFile { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="tableName"></param>
        public SubscribeContext(OptionsBuilder<T> options, string tableName)
        {
            this.Options = options;
            this.TableName = tableName;
            this.VolumeFile = new VolumeFile<T>(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            VolumeFile?.Dispose();
        }
    }
}
