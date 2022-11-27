using Kogel.Subscribe.Mssql.Entites;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Kogel.Subscribe.Mssql
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Subscribe<T> : ISubscribe<T>
        where T : class
    {

        /// <summary>
        /// 
        /// </summary>
        private readonly OptionsBuilder<T> _options;

        /// <summary>
        /// 
        /// </summary>
        private readonly List<Thread> _analysisThreads;

        /// <summary>
        /// 订阅
        /// </summary>
        public Subscribe()
        {
            _options = new OptionsBuilder<T>();
            _analysisThreads = new List<Thread>();
            OnConfiguring(_options);
            //启动解析监听
            //单表
            if (_options.Shards == null || !_options.Shards.Any())
            {
                _analysisThreads.Add(new Thread(() =>
                {
                    var analysis = new HandleAnalysis<T>();
                    analysis.Register(this, _options);
                }));
            }
            else
            {
                //分表
                foreach (var shardsTable in _options.Shards)
                {
                    _analysisThreads.Add(new Thread(() =>
                    {
                        var analysis = new HandleAnalysis<T>();
                        analysis.Register(this, _options, shardsTable);
                    }));
                }
            }
            _analysisThreads.ForEach((thread) =>
            {
                thread.IsBackground = true;
                thread.Start();
            });
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="builder"></param>
        public abstract void OnConfiguring(OptionsBuilder<T> builder);

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="messageList"></param>
        public virtual void Subscribes(List<SubscribeMessage<T>> messageList)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _analysisThreads?.ForEach(thread => thread?.Abort());
        }
    }
}
