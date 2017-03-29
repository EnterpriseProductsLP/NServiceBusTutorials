using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

using NServiceBusTutorials.ActivePassive.Common;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;
using NServiceBusTutorials.Common;

namespace NServiceBusTutorials.ActivePassive.Consumer
{
    internal class SqlServerDistributedLockManager : IManageDistributedLocks
    {
        public Task<bool> GetOrMaintainLock()
        {
            return Task.Run(() => GetOrMaintainLockInternal());
        }

        public Task ReleaseLock()
        {
            return Task.Run(() => ReleaseLockInternal());
        }

        private bool GetOrMaintainLockInternal()
        {
            var success = false;
            using (var connection = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        var pKey = new SqlParameter(parameterName: "@pKey", dbType: SqlDbType.VarChar, size: 100)
                                      {
                                          Value = ConfigurationProvider.DistributedLockKey,
                                          Direction = ParameterDirection.Input
                                      };

                        var pDiscriminator = new SqlParameter(parameterName: "@pDiscriminator", dbType: SqlDbType.VarChar, size: 100)
                                                {
                                                    Value = ConfigurationProvider.DistributedLockDiscriminator,
                                                    Direction = ParameterDirection.Input
                                                };

                        var pHeartbeatDuraction = new SqlParameter(parameterName: "@pHeartbeatDuration", dbType: SqlDbType.Int)
                                                    {
                                                        Value = ConfigurationProvider.DistributedLockDuration,
                                                        Direction = ParameterDirection.Input
                                                    };

                        var pSuccess = new SqlParameter(parameterName: "@success", dbType: SqlDbType.Bit)
                                          {
                                              Direction = ParameterDirection.Output
                                          };

                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "[Framework].[uspHeartbeatDistributedLock]";
                        command.Parameters.Add(pKey);
                        command.Parameters.Add(pDiscriminator);
                        command.Parameters.Add(pHeartbeatDuraction);
                        command.Parameters.Add(pSuccess);
                        command.ExecuteNonQuery();

                        success = (bool)pSuccess.Value;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleUtilities.WriteLineWithColor($"GetOrMaintainLock failed:  {ex.Message}", ConsoleColor.Red);
                }
                finally
                {
                    connection.Close();
                }
            }

            return success;
        }

        private void ReleaseLockInternal()
        {
            using (var connection = new SqlConnection(ConfigurationProvider.ConnectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        var key = new SqlParameter(parameterName: "@pKey", dbType: SqlDbType.VarChar, size: 100)
                                      {
                                          Value = ConfigurationProvider.DistributedLockKey,
                                          Direction = ParameterDirection.Input
                                      };

                        var discriminator = new SqlParameter(parameterName: "@pDiscriminator", dbType: SqlDbType.VarChar, size: 100)
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
                catch (Exception ex)
                {
                    ConsoleUtilities.WriteLineWithColor($"ReleaseLock failed:  {ex.Message}", ConsoleColor.Red);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}