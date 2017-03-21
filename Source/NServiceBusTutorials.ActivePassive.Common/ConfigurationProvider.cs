using System.Configuration;

namespace NServiceBusTutorials.ActivePassive.Common
{
    public class ConfigurationProvider
    {
        private static string _distributedLockDiscriminator;

        private static int? _distributedLockDuration;

        private static string _distributedLockKey;

        private static string _connectionString;

        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString = ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
                }

                return _connectionString;
            }
        }

        public static string DistributedLockDiscriminator
        {
            get
            {
                if (_distributedLockDiscriminator == null)
                {
                    _distributedLockDiscriminator = ConfigurationManager.AppSettings["DistributedLockDiscriminator"];
                }

                return _distributedLockDiscriminator;
            }
        }

        public static string DistributedLockKey
        {
            get
            {
                if (_distributedLockKey == null)
                {
                    _distributedLockKey = ConfigurationManager.AppSettings["DistributedLockKey"];
                }

                return _distributedLockKey;
            }
        }

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
