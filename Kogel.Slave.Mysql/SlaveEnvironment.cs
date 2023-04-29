using Kogel.Slave.Mysql.Extensions;
using System;

namespace Kogel.Slave.Mysql
{
    internal static class SlaveEnvironment
    {
        private static Version? _version;

        public static Version GetVersionEnvironmentVariable(string key = "mysql-v")
        {
            if (!_version.HasValue)
            {
                var versionDesc = Environment.GetEnvironmentVariable("mysql-v");
                _version = EnumExtensions.GetEnumValueFromDescription<Version>(versionDesc);
            }
            return _version.Value;
        }

        public static void SetVersionEnvironmentVariable(this Version version)
        {
            _version = version;
            string versionDesc = version.GetDescription();
            Environment.SetEnvironmentVariable("mysql-v", versionDesc);
        }
    }
}
