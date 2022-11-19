using Kogel.Subscribe.Mssql.Entites;
using System.Collections.Generic;
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
        private readonly SubscribeAnalysis<T> _analysis;

        /// <summary>
        /// 
        /// </summary>
        private readonly OptionsBuilder _options;

        /// <summary>
        /// 
        /// </summary>
        private readonly Thread _analysisThread;

        /// <summary>
        /// 订阅
        /// </summary>
        public Subscribe()
        {
            _options = new OptionsBuilder();
            OnConfiguring(_options);
            //启动解析监听
            _analysis = new SubscribeAnalysis<T>();
            _analysisThread = new Thread(() =>
            {
                _analysis.Register(this, _options);
            })
            {
                IsBackground = true
            };
            _analysisThread.Start();
        }

        /// <summary>
        /// 配置
        /// </summary>
        /// <param name="builder"></param>
        public abstract void OnConfiguring(OptionsBuilder builder);

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
            _analysis?.Dispose();
            _analysisThread?.Abort();
        }
    }
}
