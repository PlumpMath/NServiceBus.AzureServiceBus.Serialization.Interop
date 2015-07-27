namespace MessageSender
{
    using System.Collections.Generic;
    using System.Configuration;
    using NServiceBus.Config;
    using NServiceBus.Config.ConfigurationSource;

    public class EndpointMappingConfigurationSource : IConfigurationSource
    {
        readonly IEnumerable<MessageEndpointMapping> mappings;

        public EndpointMappingConfigurationSource(IEnumerable<MessageEndpointMapping> mappings)
        {
            this.mappings = mappings;
        }

        public T GetConfiguration<T>() where T : class, new()
        {
            if (typeof(T) != typeof(UnicastBusConfig))
            {
                return ConfigurationManager.GetSection(typeof(T).Name) as T;
            }

            var config = GetUnicastBusConfig();

            foreach (var messageEndpointMapping in mappings)
            {
                config.MessageEndpointMappings.Add(messageEndpointMapping);
            }
            return config as T;
        }

        static UnicastBusConfig GetUnicastBusConfig()
        {
            var config = (UnicastBusConfig) ConfigurationManager.GetSection(typeof(UnicastBusConfig).Name) 
                ?? new UnicastBusConfig
            {
                MessageEndpointMappings = new MessageEndpointMappingCollection()
            };
            return config;
        }
    }
}