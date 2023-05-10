namespace Kogel.Slave.Mysql
{

    class DefaultEventFactory<TEventType> : ILogEventFactory
        where TEventType : LogEvent, new()
    {
        public LogEvent Create(object context)
        {
            return new TEventType();
        }
    }
}
