using NServiceBus;

namespace ReusingTransportConnection
{
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            #region Transport
            configuration.UseTransport<SqlServerTransport>();
            #endregion
        }
    }
}