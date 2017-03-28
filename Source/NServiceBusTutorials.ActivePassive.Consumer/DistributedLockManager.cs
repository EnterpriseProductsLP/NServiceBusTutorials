using System;
using System.Data;
using System.Data.SqlClient;

using NServiceBusTutorials.ActivePassive.Common;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.ActivePassive.Consumer
{
    internal class DistributedLockManager : IManageDistributedLocks
    {
        private readonly bool _randomlyFail;

        private readonly Random _random;

        public DistributedLockManager(bool randomlyFail = true)
        {
            _randomlyFail = randomlyFail;
            _random = new Random(DateTime.Now.Millisecond);
        }

        public bool GetOrMaintainLock()
        {
            if (RandomlyFail())
            {
                return false;
            }

            bool result;
            using (var connection = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        var key = new SqlParameter("@pKey", SqlDbType.VarChar, 100)
                                      {
                                          Value = ConfigurationProvider.DistributedLockKey,
                                          Direction = ParameterDirection.Input
                                      };

                        var discriminator = new SqlParameter("@pDiscriminator", SqlDbType.VarChar, 100)
                                                {
                                                    Value = ConfigurationProvider.DistributedLockDiscriminator,
                                                    Direction = ParameterDirection.Input
                                                };

                        var heartbeatDuration = new SqlParameter("@pHeartbeatDuration", SqlDbType.Int)
                                                    {
                                                        Value = ConfigurationProvider.DistributedLockDuration,
                                                        Direction = ParameterDirection.Input
                                                    };

                        var success = new SqlParameter("@success", SqlDbType.Bit)
                                          {
                                              Direction = ParameterDirection.Output
                                          };

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "[Framework].[uspHeartbeatDistributedLock]";
                        command.Parameters.Add(key);
                        command.Parameters.Add(discriminator);
                        command.Parameters.Add(heartbeatDuration);
                        command.Parameters.Add(success);
                        command.ExecuteNonQuery();

                        result = (bool)success.Value;
                    }
                }
                finally
                {
                    connection.Close();
                }
            }

            return result;
        }

        private bool RandomlyFail()
        {
            if (!_randomlyFail)
            {
                return false;
            }

            var nextRandom = _random.Next(101, 501);
            if (nextRandom % 100 != 0)
            {
                return false;
            }

            ConsoleUtilities.WriteLineWithColor($"Randomly failing:  {nextRandom}", ConsoleColor.Blue);
            return true;
        }

        public void ReleaseLock()
        {
            using (var connection = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        var key = new SqlParameter("@pKey", SqlDbType.VarChar, 100)
                                      {
                                          Value = ConfigurationProvider.DistributedLockKey,
                                          Direction = ParameterDirection.Input
                                      };

                        var discriminator = new SqlParameter("@pDiscriminator", SqlDbType.VarChar, 100)
                                                {
                                                    Value = ConfigurationProvider.DistributedLockDiscriminator,
                                                    Direction = ParameterDirection.Input
                                                };

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "[Framework].[uspRemoveDistributedLock]";
                        command.Parameters.Add(key);
                        command.Parameters.Add(discriminator);
                        command.ExecuteNonQuery();
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}