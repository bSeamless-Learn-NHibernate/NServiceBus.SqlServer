namespace NServiceBus.Features
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Transactions;
    using NServiceBus.ObjectBuilder;
    using Pipeline;
    using Settings;
    using Support;
    using Transports;
    using Transports.SQLServer;
    using System.Configuration;

    class SqlServerTransportFeature : ConfigureTransport
    {
        public const string UseCallbackReceiverSettingKey = "SqlServer.UseCallbackReceiver";
        public const string PerEndpointConnectionStringsCollectionSettingKey = "SqlServer.PerEndpointConnectrionStringsCollection";
        public const string PerEndpointConnectionStringsCallbackSettingKey = "SqlServer.PerEndpointConnectrionStringsCallback";
        public const string DefaultSchemaSettingsKey = "SqlServer.SchemaName";
        public const string PrimaryPollIntervalSettingsKey = "SqlServer.PrimaryPollInterval";
        public const string SecondaryPollIntervalSettingsKey = "SqlServer.SecondaryPollInterval";

        public SqlServerTransportFeature()
        {
            Defaults(s =>
            {
                s.SetDefault(UseCallbackReceiverSettingKey, true);
                s.SetDefault(PrimaryPollIntervalSettingsKey, 1000);
                s.SetDefault(SecondaryPollIntervalSettingsKey, 2000);
            });
        }

        protected override string ExampleConnectionStringForErrorMessage
        {
            get { return @"Data Source=.\SQLEXPRESS;Initial Catalog=nservicebus;Integrated Security=True"; }
        }

        protected override RegisterStep GetReceiveBehaviorRegistration(ReceiveOptions receiveOptions)
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = receiveOptions.Transactions.IsolationLevel,
                Timeout = receiveOptions.Transactions.TransactionTimeout
            };

            if (receiveOptions.Transactions.IsTransactional)
            {
                if (receiveOptions.Transactions.SuppressDistributedTransactions)
                {
                    return new NativeTransactionReceiveBehavior.Registration(receiveOptions.ErrorQueue, transactionOptions);
                }
                return new AmbientTransactionReceiveBehavior.Registration(receiveOptions.ErrorQueue, transactionOptions);
            }
            return new NoTransactionReceiveBehavior.Registration(receiveOptions.ErrorQueue);
        }

        protected override string GetLocalAddress(ReadOnlySettings settings)
        {
            return settings.EndpointName();
        }

        protected override void Configure(FeatureConfigurationContext context, string connectionStringWithSchema)
        {
            //Until we refactor the whole address system
            Address.IgnoreMachineName();

            if (String.IsNullOrEmpty(connectionStringWithSchema))
            {
                throw new ArgumentException("Sql Transport connection string cannot be empty or null.");
            }
            var defaultSchema = context.Settings.GetOrDefault<string>(DefaultSchemaSettingsKey);

            string configStringSchema;
            var connectionString = connectionStringWithSchema.ExtractSchemaName(out configStringSchema);

            var primaryPollInterval = GetSetting<int>(context.Settings, PrimaryPollIntervalSettingsKey);
            var secondaryPollInterval = GetSetting<int>(context.Settings, SecondaryPollIntervalSettingsKey);

            var localConnectionParams = new LocalConnectionParams(configStringSchema, connectionString, defaultSchema, primaryPollInterval, secondaryPollInterval);
            context.Container.RegisterSingleton(localConnectionParams);

            var useCallbackReceiver = context.Settings.Get<bool>(UseCallbackReceiverSettingKey);

            var queueName = GetLocalAddress(context.Settings);
            var callbackQueue = string.Format("{0}.{1}", queueName, RuntimeEnvironment.MachineName);

            var connectionStringProvider = ConfigureConnectionStringProvider(context, localConnectionParams);

            var container = context.Container;

            container.ConfigureComponent<SqlServerQueueCreator>(DependencyLifecycle.InstancePerCall);

            var senderConfig = container.ConfigureComponent<SqlServerMessageSender>(DependencyLifecycle.InstancePerCall)
                .ConfigureProperty(p => p.ConnectionStringProvider, connectionStringProvider);

            container.ConfigureComponent<SqlServerPollingDequeueStrategy>(DependencyLifecycle.InstancePerCall);

            ConfigurePurging(context.Settings, container);

            context.Container.ConfigureComponent(b => new SqlServerStorageContext(b.Build<PipelineExecutor>(), localConnectionParams.ConnectionString), DependencyLifecycle.InstancePerUnitOfWork);

            if (useCallbackReceiver)
            {
                senderConfig.ConfigureProperty(p => p.CallbackQueue, callbackQueue);

                var callbackAddress = Address.Parse(callbackQueue);

                context.Container.ConfigureComponent<CallbackQueueCreator>(DependencyLifecycle.InstancePerCall)
                    .ConfigureProperty(p => p.Enabled, true)
                    .ConfigureProperty(p => p.CallbackQueueAddress, callbackAddress);

                context.Pipeline.Register<ReadCallbackAddressBehavior.Registration>();
            }
            context.Container.RegisterSingleton(new SecondaryReceiveConfiguration(workQueue =>
            {
                //if this isn't the main queue we shouldn't use callback receiver
                if (!useCallbackReceiver || workQueue != queueName)
                {
                    return SecondaryReceiveSettings.Disabled();
                }

                return SecondaryReceiveSettings.Enabled(callbackQueue);
            }));
        }

        static T GetSetting<T>(ReadOnlySettings settings, string key)
        {
            var stringValue = ConfigurationManager.AppSettings.Get("NServiceBus/" + key.Replace(".", "/"));
            if (stringValue != null)
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                return (T)converter.ConvertFromInvariantString(stringValue);
            }
            return settings.GetOrDefault<T>(key);
        }

        static CompositeConnectionStringProvider ConfigureConnectionStringProvider(FeatureConfigurationContext context, LocalConnectionParams defaultConnectionParams)
        {
            const string transportConnectionStringPrefix = "NServiceBus/Transport/";
            var configConnectionStrings =
                ConfigurationManager
                    .ConnectionStrings
                    .Cast<ConnectionStringSettings>()
                    .Where(x => x.Name.StartsWith(transportConnectionStringPrefix))
                    .Select(x =>
                    {
                        string schema;
                        var connectionString = x.ConnectionString.ExtractSchemaName(out schema);
                        var endpoint = x.Name.Replace(transportConnectionStringPrefix, String.Empty);
                        return EndpointConnectionInfo.For(endpoint).UseConnectionString(connectionString).UseSchema(schema);
                    });

            var configProvidedPerEndpointConnectionStrings = new CollectionConnectionStringProvider(configConnectionStrings, defaultConnectionParams);
            var programmaticallyProvidedPerEndpointConnectionStrings = CreateProgrammaticPerEndpointConnectionStringProvider(context, defaultConnectionParams);

            var connectionStringProvider = new CompositeConnectionStringProvider(
                configProvidedPerEndpointConnectionStrings,
                programmaticallyProvidedPerEndpointConnectionStrings,
                new DefaultConnectionStringProvider(defaultConnectionParams)
                );
            return connectionStringProvider;
        }

        static IConnectionStringProvider CreateProgrammaticPerEndpointConnectionStringProvider(FeatureConfigurationContext context, LocalConnectionParams defaultConnectionParams)
        {
            var collection = context.Settings.GetOrDefault<IEnumerable<EndpointConnectionInfo>>(PerEndpointConnectionStringsCollectionSettingKey);
            if (collection != null)
            {
                return new CollectionConnectionStringProvider(collection, defaultConnectionParams);
            }
            var callback = context.Settings.GetOrDefault<Func<string, ConnectionInfo>>(PerEndpointConnectionStringsCallbackSettingKey);
            if (callback != null)
            {
                return new DelegateConnectionStringProvider(callback, defaultConnectionParams);
            }
            return new NullConnectionStringProvider();
        }

        static void ConfigurePurging(ReadOnlySettings settings, IConfigureComponents container)
        {
            bool purgeOnStartup;
            if (settings.TryGet("Transport.PurgeOnStartup", out purgeOnStartup) && purgeOnStartup)
            {
                container.ConfigureComponent<QueuePurger>(DependencyLifecycle.SingleInstance);
            }
            else
            {
                container.ConfigureComponent<NullQueuePurger>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}