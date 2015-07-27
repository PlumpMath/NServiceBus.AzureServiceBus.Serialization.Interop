namespace MessageReceiver.Handlers
{
    using System;
    using MessageContracts;
    using NServiceBus;

    public class TestMessageHandler : IHandleMessages<TestMessage>
    {
        public void Handle(TestMessage message)
        {
            Console.WriteLine();
            Console.WriteLine(@"Id: {0}", message.Id);
            Console.WriteLine(@"Text: {0}", message.Text);
            Console.WriteLine();
        }
    }
}
