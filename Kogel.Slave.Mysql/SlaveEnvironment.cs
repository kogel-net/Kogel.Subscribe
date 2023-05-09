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
                var version = Environment.GetEnvironmentVariable(key);
                _version = (Version)Convert.ToInt32(version);
            }
            return _version.Value;
        }

        public static void SetVersionEnvironmentVariable(this Version version, string key = "mysql-v")
        {
            _version = version;
            Environment.SetEnvironmentVariable(key, version.ToString());
        }
    }
}
