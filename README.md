# The Problem
If your implementation of NServiceBus with Azure Service Bus Transport results in the below exception message:

  "There was an error deserializing the object of type System.Byte[]"
  
The root problem is with BrokeredMessage. Underlying GetBody<T>(), and setting the backing payload as a constructor argument, it uses an implementation of DataContractSerializer. When the Transport set the body to type byte[] and retrieved it using GetBody<byte[]>(), WindowsAzure.ServiceBus uses a [Binary] XMLDictionary[Reader|Writer] to serialize and deserialize the payload.

In order to get around this, you must use streams with BrokeredMessage.

## The Solution
This example shows how to have interop serialization between Java and .NET over AzureServiceBus transport in NServiceBus using JSON. 

# Implementation
This example uses the **temporary** BrokeredMessageBodyConversion point in v6.3.2 of NServiceBus.Azure.Transports.WindowsAzureServiceBus
in order to support Java to .NET serialization over Azure Service Bus Transport.

Please refer to the official [NServiceBus.AzureServiceBus](https://github.com/Particular/NServiceBus.AzureServiceBus) repository for progress on the next release. The issue that spawned this example can be found [here](https://github.com/Particular/NServiceBus.AzureServiceBus/issues/49).

## Extension Point

I created an extension of the transport extension point to keep the bus configuration clean:

```csharp
public static class AzureServiceBusTransportExtensions
{
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
  ```
  
Then I created two extensions for the BrokeredMessage interception:

```csharp
public static class BrokeredMessageExtensions
{
    public static byte[] ToBytes(this BrokeredMessage message)
    {
        using (var stream = new MemoryStream())
        using (var body = message.GetBody<Stream>())
        {
            body.CopyTo(stream);
            return stream.ToArray();
        }
    }

    public static BrokeredMessage ToBrokeredMessage(this byte[] bytes)
    {
        return bytes != null ? new BrokeredMessage(new MemoryStream(bytes)) : new BrokeredMessage();
    }
}
```

The finally configuration looks like:

```csharp
config.UseTransport<AzureServiceBusTransport>()
  .ConnectionString(settings.GetConnectionString())
  .BrokeredMessageBodyInterceptors(m => m.ToBytes(), b => b.ToBrokeredMessage());
```
