namespace MessageSender.Messaging
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    /// <summary>
    /// the type of token to be used for security
    /// </summary>
    public enum TokenType
    {
        SharedAccessSignature
    }

    /// <summary>
    /// separate out the creation of the security token. For this poc, it uses the SAS 
    /// <a href="https://msdn.microsoft.com/en-us/library/azure/dn170477.aspx">here</a>
    /// </summary>
    public class TokenFactory
    {
        private readonly string endpointUri;
        private readonly string keyValue;
        private readonly string keyName;

        private static string sasSignatureFormat = "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}";

        public TokenFactory(string endpointUri, string keyValue, string keyName)
        {
            this.endpointUri = endpointUri;
            this.keyValue = keyValue;
            this.keyName = keyName;
        }

        public string Create(TokenType tokenType)
        {
            if (tokenType == TokenType.SharedAccessSignature)
            {
                return CreateSasToken();
            }

            //just making it not implemented for now.
            throw new ArgumentOutOfRangeException();
        }

        private string CreateSasToken()
        {
            // Set token lifetime to 20 minutes.
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var diff = DateTime.Now.ToUniversalTime() - origin;

            uint tokenExpirationTime = Convert.ToUInt32(diff.TotalSeconds) + 20 * 60;

            var encoding = Encoding.UTF8;
            var hmac = new HMACSHA256(encoding.GetBytes(keyValue));

            var encodedUri = HttpUtility.UrlEncode(endpointUri);

            var bytesToSign = encoding.GetBytes(encodedUri + "\n" + tokenExpirationTime);
            var signature = HttpUtility.UrlEncode(Convert.ToBase64String(hmac.ComputeHash(bytesToSign)));

            var token = string.Format(sasSignatureFormat, 
                encodedUri, signature, tokenExpirationTime, keyName);

            return token;
        }
    }
}