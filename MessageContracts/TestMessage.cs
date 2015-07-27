namespace MessageContracts
{
    using System;
    using NServiceBus;

    public class TestMessage : ICommand
    {
        public Guid Id { get; set; }
        public string Text { get; set; }

        public TestMessage()
        {
            Id = new Guid("4b88e7fd-e3f7-4471-b5d9-571e37073212");
        }
    }
}