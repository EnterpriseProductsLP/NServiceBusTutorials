using System;
using System.Data;
using System.Data.SqlClient;
using System.Timers;

using NServiceBus;

using NServiceBusTutorials.ActivePassive.Common;
using NServiceBusTutorials.ActivePassive.Contracts;
using NServiceBusTutorials.Common;
using NServiceBusTutorials.Common.Extensions;

using Timer = System.Timers.Timer;

namespace NServiceBusTutorials.ActivePassive.Consumer1
{
    internal class WorkConsumer : Worker
    {
        private readonly EndpointConfigurationBuilder _endpointConfigurationBuilder;

        private readonly Timer _heartbeatTimer = new Timer(2000);

        private readonly Timer _startupTimer = new Timer(10000);

        private IEndpointInstance _endpointInstance;

        public WorkConsumer(EndpointConfigurationBuilder endpointConfigurationBuilder)
        {
            _endpointConfigurationBuilder = endpointConfigurationBuilder;
            _heartbeatTimer.Elapsed += OnHeartbeatTimerElapsed;
            _startupTimer.Elapsed += OnStartupTimerElapsed;
        }

        protected override void OnPausing()
        {
            _heartbeatTimer.Stop();
            StopEndpoint();
            SetPaused();
            _startupTimer.Start();
        }

        protected override void OnResuming()
        {
            try
            {
                _startupTimer.Stop();
                StartEndpoint();
                SetRunning();
                _heartbeatTimer.Start();
            }
            catch
            {
                Pause();
            }
        }

        protected override void OnRunning()
        {
            _startupTimer.Start();
        }

        protected override void OnStopping()
        {
            StopEndpoint();
        }

        protected override void DoStep()
        {
        }

        private void OnHeartbeatTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                // TODO:  Remove this line when done debugging.
                _heartbeatTimer.Stop();

                Heartbeat();

                // TODO:  Remove this line when done debugging.
                _heartbeatTimer.Start();
            }
            catch
            {
                Pause();
            }
        }

        private void OnStartupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                _startupTimer.Stop();
                Heartbeat();
                Resume();
            }
            catch
            {
                _startupTimer.Start();
            }
        }

        private void Heartbeat()
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
                                          Value =
                                              ConfigurationProvider
                                              .DistributedLockKey,
                                          Direction =
                                              ParameterDirection.Input
                                      };
                        var discriminator = new SqlParameter("@pDiscriminator", SqlDbType.VarChar, 100)
                                                {
                                                    Value =
                                                        ConfigurationProvider
                                                        .DistributedLockDiscriminator,
                                                    Direction =
                                                        ParameterDirection
                                                        .Input
                                                };
                        var heartbeatDuration = new SqlParameter("@pHeartbeatDuration", SqlDbType.Int)
                                                    {
                                                        Value =
                                                            ConfigurationProvider
                                                            .DistributedLockDuration,
                                                        Direction =
                                                            ParameterDirection
                                                            .Input
                                                    };
                        var success = new SqlParameter("@success", SqlDbType.Bit)
                                          {
                                              Direction =
                                                  ParameterDirection.Output
                                          };
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandText = "[Framework].[uspHeartbeatDistributedLock]";
                        command.Parameters.Add(key);
                        command.Parameters.Add(discriminator);
                        command.Parameters.Add(heartbeatDuration);
                        command.Parameters.Add(success);
                        command.ExecuteNonQuery();

                        var result = (bool)success.Value;

                        if (result != true)
                        {
                            throw new Exception("Heartbeat failed");
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void StartEndpoint()
        {
            var endpointConfiguration = _endpointConfigurationBuilder.GetEndpointConfiguration(endpointName: Endpoints.Consumer, auditQueue: Endpoints.AuditQueue, errorQueue: Endpoints.ErrorQueue);
            var startableEndpoint = Endpoint.Create(endpointConfiguration).Inline();
            _endpointInstance = startableEndpoint.Start().Inline();
        }

        private void StopEndpoint()
        {
            if (_endpointInstance == null)
            {
                return;
            }

            _endpointInstance.Stop().Inline();
            _endpointInstance = null;
        }
    }
}
