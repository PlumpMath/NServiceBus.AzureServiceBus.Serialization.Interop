namespace MessageReceiver
{
    using System;
    using System.Threading;
    using InteropExtensions;
    using NServiceBus;

    class Program
    {
        static readonly ManualResetEvent manualReset = new ManualResetEvent(false);

        // TODO: replace with real values
        private static readonly AzureServiceBusSettings settings = new AzureServiceBusSettings();

        private const string EndpointName = "testmessage";

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (o, e) =>
            {
                manualReset.Set();
                e.Cancel = true;
            };

            StartBus();
            Console.Clear();
            Console.WriteLine("Waiting for messages...");

            manualReset.WaitOne();
        }

        static void StartBus()
        {
            var config = new BusConfiguration();
            config.EndpointName(EndpointName);
            config.ScaleOut().UseSingleBrokerQueue();

            config.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(settings.GetConnectionString())
                .BrokeredMessageBodyInterceptors(m => m.ToBytes(), b => b.ToBrokeredMessage());

            config.UseSerialization<JsonSerializer>();
            config.UsePersistence<InMemoryPersistence>();

            Bus.Create(config).Start();
        }
    }

    public class AzureServiceBusSettings
    {
        private const string ConnectionStringFormat = "Endpoint=sb://{0}.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2}";


        public string Namespace { get; private set; }
        public string AccessKeyName { get; private set; }
        public string AccessKey { get; private set; }

        public AzureServiceBusSettings()
        {
            //TODO: replace with your values

            Namespace = "nsb-serialization-interop";
            AccessKeyName = "RootManageSharedAccessKey";
            AccessKey = "[value here of the key]";
        }

        public string GetConnectionString()
        {
            return string.Format(ConnectionStringFormat, Namespace, AccessKeyName, AccessKey);
        }
    }
}
