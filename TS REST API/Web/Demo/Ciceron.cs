namespace Demo
{
    public class Ciceron
    {
#pragma warning disable CS8618
        public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
        }

        public class UserAttributes
        {
            public string serialNumber { get; set; }
            public string CN { get; set; }
            public string issuerCommonName { get; set; }
            public string Signature { get; set; }
            public string Timestamp { get; set; }
            public string TransactionId { get; set; }
            public string SignDigest { get; set; }
            public string SignMessage { get; set; }

        }

        public class Response
        {
            public Error errorObject { get; set; }
            public string redirectUrl { get; set; }
            public UserAttributes userAttributes { get; set; }
            public string username { get; set; }
        }
#pragma warning restore CS8618

    }
}
