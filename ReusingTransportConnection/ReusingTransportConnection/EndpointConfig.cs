using NServiceBus;

namespace Sample.SqlServer.NoDTC
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.UseTransport<SqlServerTransport>();
        }
    }
}