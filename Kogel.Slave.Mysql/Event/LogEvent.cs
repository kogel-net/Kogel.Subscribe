using Kogel.Slave.Mysql.Entites.Enum;
using Kogel.Slave.Mysql.Extension;
using Kogel.Slave.Mysql.Extension.DataType;
using Kogel.Slave.Mysql.Interface;
using System;

namespace Kogel.Slave.Mysql.Event
{
   public abstract class LogEvent
    {
        /// <summary>
        /// 校验方式
        /// </summary>
        public static CheckSumEnum BinLogCheckSum { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public LogEventTypeEnum LogEventType { get; set; }

        /// <summary>
        /// 从节点id（需要保持唯一）
        /// </summary>
        public int ServerId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LogEventSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int LogPosition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public LogEventFlagEnum LogEventFlag { get; set; }

        /// <summary>
        /// 
        /// </summary>

        internal static IDataType[] DataTypes { get; private set; } = new IDataType[256];

        static LogEvent()
        {
            DataTypes[(int)DataTypeEnum.BIT] = new BitType();
            DataTypes[(int)DataTypeEnum.BLOB] = new BlobType();
            DataTypes[(int)DataTypeEnum.DATETIME] = new DateTimeType();
            DataTypes[(int)DataTypeEnum.DATETIME_V2] = new DateTimeV2Type();
            DataTypes[(int)DataTypeEnum.DATE] = new DateType();
            DataTypes[(int)DataTypeEnum.DOUBLE] = new DoubleType();
            DataTypes[(int)DataTypeEnum.ENUM] = new EnumType();
            DataTypes[(int)DataTypeEnum.FLOAT] = new FloatType();
            DataTypes[(int)DataTypeEnum.GEOMETRY] = new GeometryType();
            DataTypes[(int)DataTypeEnum.INT24] = new Int24Type();
            DataTypes[(int)DataTypeEnum.JSON] = new JsonType();
            DataTypes[(int)DataTypeEnum.LONGLONG] = new LongLongType();
            DataTypes[(int)DataTypeEnum.NEWDECIMAL] = new NewDecimalType();
            DataTypes[(int)DataTypeEnum.SET] = new SetType();
            DataTypes[(int)DataTypeEnum.SHORT] = new ShortType();
            DataTypes[(int)DataTypeEnum.STRING] = new StringType();
            DataTypes[(int)DataTypeEnum.TIMESTAMP] = new TimestampType();
            DataTypes[(int)DataTypeEnum.TIMESTAMP_V2] = new TimestampV2Type();
            DataTypes[(int)DataTypeEnum.TIME] = new TimeType();
            DataTypes[(int)DataTypeEnum.TIME_V2] = new TimeV2Type();
            DataTypes[(int)DataTypeEnum.TINY] = new TinyType();
            DataTypes[(int)DataTypeEnum.VARCHAR] = new VarCharType();
            DataTypes[(int)DataTypeEnum.YEAR] = new YearType();
        }

        protected internal abstract void DecodeBody(ref SequenceReader<byte> reader, object context);

        public const int MARIA_SLAVE_CAPABILITY_GTID = 4;
        public const int MARIA_SLAVE_CAPABILITY_MINE = MARIA_SLAVE_CAPABILITY_GTID;

        private static readonly DateTime _unixEpoch = new DateTime(1970, 1, 1);

        internal static DateTime GetTimestampFromUnixEpoch(int seconds)
        {
            return _unixEpoch.AddSeconds(seconds);
        }

        protected bool HasCRC { get; set; } = false;

        protected bool RebuildReaderAsCRC(ref SequenceReader<byte> reader)
        {
            if (!HasCRC || BinLogCheckSum == CheckSumEnum.NONE)
                return false;

            reader = new SequenceReader<byte>(reader.Sequence.Slice(reader.Consumed, reader.Remaining - (int)BinLogCheckSum));
            return true;
        }

        public override string ToString()
        {
            return LogEventType.ToString();
        }
    }
}
