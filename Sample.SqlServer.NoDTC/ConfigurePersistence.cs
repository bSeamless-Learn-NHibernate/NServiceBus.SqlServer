using System.Collections.Generic;
using System.Configuration;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NServiceBus;
using NServiceBus.Persistence;
using Sample.SqlServer.NoDTC.Entities;
using Configuration = NHibernate.Cfg.Configuration;

namespace Sample.SqlServer.NoDTC
{
    class ConfigurePersistence : INeedInitialization
    {
        public void Customize(BusConfiguration config)
        {
            var configuration = BuildConfiguration();

            config
                .UsePersistence<NHibernatePersistence>()
                .UseConfiguration(configuration);
        }

        private static Configuration BuildConfiguration()
        {
            var configuration = new Configuration()
                .SetProperties(new Dictionary<string, string>
                {
                    {
                        Environment.ConnectionString,
                        ConfigurationManager.ConnectionStrings["NServiceBus/Persistence"].ConnectionString
                    },
                    {
                        Environment.Dialect,
                        "NHibernate.Dialect.MsSql2012Dialect"
                    }
                });

            var mapper = new ModelMapper();
            mapper.AddMapping<OrderMap>();
            var mappings = mapper.CompileMappingForAllExplicitlyAddedEntities();
            configuration.AddMapping(mappings);

            return configuration;
        }
    }
}