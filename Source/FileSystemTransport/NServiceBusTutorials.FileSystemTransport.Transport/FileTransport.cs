using NServiceBus.Settings;
using NServiceBus.Transport;

namespace NServiceBusTutorials.FileSystemTransport.Transport
{
    public class FileTransport : TransportDefinition
    {
        public override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            return new FileTransportInfrastructure();
        }

        public override string ExampleConnectionStringForErrorMessage => "";
    }
}
