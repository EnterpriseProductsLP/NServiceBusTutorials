using System.Configuration;

namespace NServiceBusTutorials.ActivePassive.Common
{
    public class ConfigurationProvider
    {
        private static string _distributedLockDiscriminator;

        private static int? _distributedLockDuration;

        private static string _distributedLockKey;

        private static string _connectionString;

        public static string ConnectionString => _connectionString ?? (_connectionString = ConfigurationManager.ConnectionStrings[name: "DbConnection"].ConnectionString);

        public static string DistributedLockDiscriminator => _distributedLockDiscriminator ?? (_distributedLockDiscriminator = ConfigurationManager.AppSettings[name: "DistributedLockDiscriminator"]);

        public static string DistributedLockKey => _distributedLockKey ?? (_distributedLockKey = ConfigurationManager.AppSettings[name: "DistributedLockKey"]);

        public static int DistributedLockDuration
        {
            get
            {
                if (!_distributedLockDuration.HasValue)
                {
                    _distributedLockDuration = int.Parse(ConfigurationManager.AppSettings[name: "DistributedLockDuration"]);
                }

                return _distributedLockDuration.Value;
            }
        }
    }
}