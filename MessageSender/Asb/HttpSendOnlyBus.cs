namespace MessageSender.Asb
{
    using System;
    using System.Net.Http;
    using System.Text;
    using Messaging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class HttpSendOnlyBus 
    {
        private readonly string accessKeyName;
        private readonly string accessKey;
        private readonly string endpointUri;
        private const string EndpointUriFormat = "https://{0}.servicebus.windows.net/{1}/messages";

        public HttpSendOnlyBus(string namespaceName, string queueName, string accessKeyName, string accessKey)
        {
            this.accessKeyName = accessKeyName;
            this.accessKey = accessKey;
            this.endpointUri = string.Format(EndpointUriFormat, namespaceName, queueName);
        }

        public void Send(object message)
        {
            var client = GetHttpClient();
            var content = GetJsonSerializedBody(message);
            //content.Headers.Add("Content-Type", "application/json");

            var response = client.PostAsync(endpointUri, content).Result;
            response.EnsureSuccessStatusCode();
        }

        private static ByteArrayContent GetJsonSerializedBody(object message)
        {
            //serialize to json for the body, include the type info "$type" attr
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            var jsonBody = JsonConvert.SerializeObject(message, Formatting.None, settings);

            var body = Encoding.UTF8.GetBytes(jsonBody);
            var content = new ByteArrayContent(body);

            content.Headers.Add("NServiceBus.MessageIntent", "Send");//NServiceBus interop

            return content;
        }

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("ContentType", "application/octet-stream;type=entry;charset=utf-8");
            client.DefaultRequestHeaders.Add("Authorization", GetSecurityToken());

            var messageId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var headers = new JObject();
            headers["CorrelationId"] = correlationId;
            headers["TimeToLive"] = 30; //TimeToLive
            headers["MessageId"] = messageId;

            var json = JsonConvert.SerializeObject(headers);
            client.DefaultRequestHeaders.Add("BrokerProperties", json);

            return client;
        }

        private string GetSecurityToken()
        {
            var token = new TokenFactory(endpointUri, accessKey, accessKeyName)
                .Create(TokenType.SharedAccessSignature);

            return token;
        }
    }
}
