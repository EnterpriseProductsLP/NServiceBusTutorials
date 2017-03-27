using System.Configuration;

namespace NServiceBusTutorials.ActivePassive.Common
{
    public class ConfigurationProvider
    {
        private static string _distributedLockDiscriminator;

        private static int? _distributedLockDuration;

        private static string _distributedLockKey;

        private static string _connectionString;

        public static string ConnectionString => _connectionString ?? (_connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString);

        public static string DistributedLockDiscriminator => _distributedLockDiscriminator ?? (_distributedLockDiscriminator = ConfigurationManager.AppSettings["DistributedLockDiscriminator"]);

        public static string DistributedLockKey => _distributedLockKey ?? (_distributedLockKey = ConfigurationManager.AppSettings["DistributedLockKey"]);

        public static int DistributedLockDuration
        {
            get
            {
                if (!_distributedLockDuration.HasValue)
                {
                    _distributedLockDuration = int.Parse(ConfigurationManager.AppSettings["DistributedLockDuration"]);
                }

                return _distributedLockDuration.Value;
            }
        }
    }
}
