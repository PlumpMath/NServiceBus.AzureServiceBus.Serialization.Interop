namespace InteropExtensions
{
    using System;
    using System.IO;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;

    /// <summary>
    /// AzureServiceBus Transport extensions
    /// </summary>
    public static class AzureServiceBusTransportExtensions
    {
        /// <summary>
        /// Allows for interception of the <see cref="BrokeredMessage"/> body being get and set during the 
        /// process of converting it to/from an NServiceBus <see cref="TransportMessage"/>
        /// </summary>
        /// <param name="transport">The extension point</param>
        /// <param name="extractBody">The body extraction interception method</param>
        /// <param name="injectBody">The body injections interception method</param>
        /// <returns></returns>
        public static TransportExtensions BrokeredMessageBodyInterceptors(
            this TransportExtensions transport, 
            Func<BrokeredMessage, byte[]> extractBody, 
            Func<byte[], BrokeredMessage> injectBody)
        {
            BrokeredMessageBodyConversion.ExtractBody = extractBody;
            BrokeredMessageBodyConversion.InjectBody = injectBody;

            return transport;
        }
    }
}