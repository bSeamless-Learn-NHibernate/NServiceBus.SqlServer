using System.Collections.Generic;
using System.Configuration;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NServiceBus;
using NServiceBus.Persistence;
using Sample.SqlServer.NoDTC.Entities;
using Configuration = NHibernate.Cfg.Configuration;

namespace Sample.SqlServer.NoDTC
{
    class ConfigurePersistence : INeedInitialization
    {
        public void Init(Configure config)
        {
            Configuration configuration = BuildConfiguration();

            config
                .UsePersistence<NServiceBus.NHibernate>(c => c.UseConfiguration(configuration));
        }

        private static Configuration BuildConfiguration()
        {
            Configuration configuration = new Configuration()
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
            HbmMapping mappings = mapper.CompileMappingForAllExplicitlyAddedEntities();
            configuration.AddMapping(mappings);

            return configuration;
        }
    }
}