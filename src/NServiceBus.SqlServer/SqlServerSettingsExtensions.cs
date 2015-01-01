namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using Configuration.AdvanceExtensibility;
    using NServiceBus.Transports.SQLServer;

    /// <summary>
    /// Adds extra configuration for the Sql Server transport.
    /// </summary>
    public static class SqlServerSettingsExtensions
    {
        /// <summary>
        /// Configures the poll interval for the main (primary) receiver.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="pollInterval">Interval in milliseconds</param>
        /// <returns></returns>
        public static TransportExtensions<SqlServerTransport> PollInterval(this TransportExtensions<SqlServerTransport> transportExtensions, int pollInterval)
        {
            transportExtensions.GetSettings().Set(Features.SqlServerTransportFeature.PrimaryPollIntervalSettingsKey, pollInterval);
            return transportExtensions;
        }

        /// <summary>
        /// Disables the separate receiver that pulls messages from the machine specific callback queue.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <returns></returns>
        public static TransportExtensions<SqlServerTransport> DisableCallbackReceiver(this TransportExtensions<SqlServerTransport> transportExtensions) 
        {
            transportExtensions.GetSettings().Set(Features.SqlServerTransportFeature.UseCallbackReceiverSettingKey, false);
            return transportExtensions;
        }

        /// <summary>
        /// Configures the poll interval for the callback (secondary) receiver.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="pollInterval">Interval in milliseconds</param>
        /// <returns></returns>
        public static TransportExtensions<SqlServerTransport> CallbackReceiverPollInterval(this TransportExtensions<SqlServerTransport> transportExtensions, int pollInterval)
        {
            transportExtensions.GetSettings().Set(Features.SqlServerTransportFeature.SecondaryPollIntervalSettingsKey, pollInterval);
            return transportExtensions;
        }

        /// <summary>
        /// Provides per-endpoint connection strings for multi-database support.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="connectionInformationCollection">A collection of endpoint-connection info pairs</param>
        /// <returns></returns>
        public static TransportExtensions<SqlServerTransport> UseSpecificConnectionInformation(
            this TransportExtensions<SqlServerTransport> transportExtensions, 
            IEnumerable<EndpointConnectionInfo> connectionInformationCollection)
        {
            if (connectionInformationCollection == null)
            {
                throw new ArgumentNullException("connectionInformationCollection");
            }
            transportExtensions.GetSettings().Set(Features.SqlServerTransportFeature.PerEndpointConnectionStringsCollectionSettingKey, connectionInformationCollection);
            return transportExtensions;
        }

        /// <summary>
        /// Provides per-endpoint connection strings for multi-database support.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="connectionInformationCollection">A collection of endpoint-connection info pairs</param>
        /// <returns></returns>
        public static TransportExtensions<SqlServerTransport> UseSpecificConnectionInformation(
            this TransportExtensions<SqlServerTransport> transportExtensions,
            params EndpointConnectionInfo[] connectionInformationCollection)
        {
            return UseSpecificConnectionInformation(transportExtensions, (IEnumerable<EndpointConnectionInfo>)connectionInformationCollection);
        }
        
        /// <summary>
        /// Provides per-endpoint connection strings for multi-database support.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="connectionInformationProvider">A function that gets the endpoint name and returns connection information or null if default.</param>
        /// <returns></returns>
        public static TransportExtensions<SqlServerTransport> UseSpecificConnectionInformation(
            this TransportExtensions<SqlServerTransport> transportExtensions, 
            Func<string, ConnectionInfo> connectionInformationProvider)
        {
            if (connectionInformationProvider == null)
            {
                throw new ArgumentNullException("connectionInformationProvider");
            }
            transportExtensions.GetSettings().Set(Features.SqlServerTransportFeature.PerEndpointConnectionStringsCallbackSettingKey, connectionInformationProvider);
            return transportExtensions;
        }

        /// <summary>
        /// Overrides the default schema (dbo) used for queue tables.
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="schemaName">Name of the schema to use instead do dbo</param>
        /// <returns></returns>
        public static TransportExtensions<SqlServerTransport> DefaultSchema( this TransportExtensions<SqlServerTransport> transportExtensions, string schemaName)
        {
            if (schemaName == null)
            {
                throw new ArgumentNullException("schemaName");
            }
            transportExtensions.GetSettings().Set(Features.SqlServerTransportFeature.DefaultSchemaSettingsKey, schemaName);
            return transportExtensions;
        }
    }
}