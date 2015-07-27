namespace MessageSender
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Asb;
    using InteropExtensions;
    using MessageContracts;
    using NServiceBus;
    using NServiceBus.Config;

    class Program
    {
        const int SWP_NOSIZE = 0x0001;

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        private static IntPtr MyConsole = GetConsoleWindow();

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        static readonly ManualResetEvent manualReset = new ManualResetEvent(false);

        private static readonly AzureServiceBusSettings settings = new AzureServiceBusSettings();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (o, e) =>
            {
                manualReset.Set();
                e.Cancel = true;
            };

            int xpos = 10;
            int ypos = 10;
            SetWindowPos(MyConsole, 0, xpos, ypos, 0, 0, SWP_NOSIZE);

            Console.SetWindowSize(65, 25);
            Console.WindowLeft = 0;
            Console.WindowTop = 0;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.Clear();

            ProduceMessages();

            manualReset.WaitOne();
        }

        private static void ProduceMessages()
        {
            var httpSendOnlyBus = new HttpSendOnlyBus(settings.Namespace, settings.QueueName, settings.AccessKeyName, settings.AccessKey);
            var sendOnlyBus = Create();

            var count = 0;

            Console.Clear();
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("       Message Sender      ");
            Console.WriteLine();
            Console.WriteLine(" Sends ASB messages over HTTPS or Native NSB ");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("Press Enter to create a message...");


            while (true)
            {
                if (count > 0)
                    Console.WriteLine("Message Sent... \n");

                Console.Write("Select [1 = JSON over HTTP, 2 = NSB Native]: ");
                var type = int.Parse(Console.ReadLine() ?? "1");

                Console.Write("Enter text for the message: ");
                var text = Console.ReadLine();

                var msg = new TestMessage() {Text = text};

                if (type == 1)
                {
                    httpSendOnlyBus.Send(msg);
                }
                else
                {
                    sendOnlyBus.Send(msg);
                }

                count++;
            }
        }

        static ISendOnlyBus Create()
        {
            var config = new BusConfiguration();
            config.EndpointName("nsbSender");

            config.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(settings.GetConnectionString())
                .BrokeredMessageBodyInterceptors(m => m.ToBytes(), b => b.ToBrokeredMessage());

            config.UseSerialization<JsonSerializer>();
            config.UsePersistence<InMemoryPersistence>();
            config.CustomConfigurationSource(new EndpointMappingConfigurationSource(new []
            {
                new MessageEndpointMapping
                {
                    AssemblyName = "MessageContracts",
                    Endpoint = settings.QueueName
                }
            }));
            return Bus.CreateSendOnly(config);
        }
    }

    public class AzureServiceBusSettings
    {
        private const string ConnectionStringFormat = "Endpoint=sb://{0}.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2}";

        public string Namespace { get; private set; }
        public string AccessKeyName { get; private set; }
        public string AccessKey { get; private set; }
        public string QueueName { get; private set; }

        public AzureServiceBusSettings()
        {
            //TODO: replace with real values

            Namespace = "nsb-serialization-interop";
            AccessKeyName = "RootManageSharedAccessKey";
            AccessKey = "[value here of the key]";
            QueueName = "testmessage";
        }

        public string GetConnectionString()
        {
            return string.Format(ConnectionStringFormat, Namespace, AccessKeyName, AccessKey);
        }
    }
}
