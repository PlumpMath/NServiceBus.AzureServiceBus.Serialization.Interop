namespace InteropExtensions
{
    using System.IO;
    using Microsoft.ServiceBus.Messaging;

    public static class BrokeredMessageExtensions
    {
        /// <summary>
        /// Gets the raw body stream and converts it to the underlying bytes
        /// </summary>
        /// <param name="message">The message</param>
        /// <returns>Returns the "raw" body bytes</returns>
        public static byte[] ToBytes(this BrokeredMessage message)
        {
            using (var stream = new MemoryStream())
            using (var body = message.GetBody<Stream>())
            {
                body.CopyTo(stream);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Converts the "raw" (body) to a new brokered message with an underlying stream wrapping the bytes
        /// </summary>
        /// <param name="bytes">The body</param>
        /// <returns></returns>
        public static BrokeredMessage ToBrokeredMessage(this byte[] bytes)
        {
            return bytes != null ? new BrokeredMessage(new MemoryStream(bytes)) : new BrokeredMessage();
        }
    }
}