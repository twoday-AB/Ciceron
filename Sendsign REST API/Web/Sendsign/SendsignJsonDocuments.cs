using System.Text.Json.Serialization;

namespace Sendsign.JsonDocuments
{
    public class SendsignFetchResponse
    {
        public class SendsignFetchSigners
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("recipient_pnr")]
            public string? RecipientPnr { get; set; }

            [JsonPropertyName("recipient_cn")]
            public string? RecipientCn { get; set; }

            [JsonPropertyName("recipient_email")]
            public string? RecipientEmail { get; set; }

            [JsonPropertyName("recipient_mobile")]
            public string? RecipientMobile { get; set; }
        }

        public class SendsignFetchFiles
        {
            [JsonPropertyName("data")]
            public string? Data { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        [JsonPropertyName("message_status")]
        public string? MessageStatus { get; set; }

        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }

        [JsonPropertyName("files")]
        public IList<SendsignFetchFiles>? Files { get; set; }

        [JsonPropertyName("signers")]
        public IList<SendsignFetchSigners>? Signers { get; set; }
    }

    public class SendsignCookieStorage
    {
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }

    public class SendsignAttachment
    {
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("content_type")]
        public string? ContentType { get; set; }

        public static SendsignAttachment LoadFile(string filename)
        {
            return new SendsignAttachment()
            {
                Data = Convert.ToBase64String(File.ReadAllBytes(filename)),
                Name = Path.GetFileName(filename),
                ContentType = "application/pdf"
            };
        }

        public void Validate()
        {
            if (Data == null || Name == null || ContentType == null)
            {
                throw new ArgumentException("Attachment must contain at least data, name, and content_type");
            }
        }
    }

    public class SendsignCancelResponse
    {
        [JsonPropertyName("message_canceled")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MessageCancelled { get; set; }

        [JsonPropertyName("Error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }
    }

    public class SendsignRecipient
    {
        [JsonPropertyName("mail")]
        public string? Mail { get; set; }

        [JsonPropertyName("ssn")]
        public string? Ssn { get; set; }

        [JsonPropertyName("sms")]
        public string? Sms { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class SendsignRemindSignerResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("mail")]
        public string? Mail { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("customer_key")]
        public string? CustomerKey { get; set; }

        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }
    }

    public class SendsignRequest
    {
        [JsonPropertyName("message_id")]
        public string? MessageId { get; set; }
        [JsonPropertyName("customer_key")]
        public string? CustomerKey { get; set; }
        [JsonPropertyName("sender")]
        public string? Sender { get; set; }
    }

    public class SendsignSendRequest
    {
        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("customer_key")]
        public string? CustomerKey { get; set; }

        [JsonPropertyName("sender")]
        public string? Sender { get; set; }

        [JsonPropertyName("message_type")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? MessageType { get; set; }

        [JsonPropertyName("time_to_live_hours")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? TimeToLiveHours { get; set; }

        [JsonPropertyName("recipients")]
        public IList<SendsignRecipient> Recipients { get; set; }
        [JsonPropertyName("attachments")]
        public IList<SendsignAttachment> Attachments { get; set; }

        public SendsignSendRequest()
        {
            Recipients = [];
            Attachments = [];
        }

        public static SendsignSendRequest Create(
            string subject,
            string body,
            string customer_key,
            string sender,
            string message_type,
            Nullable<int> time_to_live_hours,
            IList<SendsignAttachment> attachments,
            IList<SendsignRecipient> recipients)
        {
            return new SendsignSendRequest()
            {
                Subject = subject,
                Body = body,
                CustomerKey = customer_key,
                Sender = sender,
                TimeToLiveHours = time_to_live_hours,
                Attachments = attachments,
                Recipients = recipients,
                MessageType = message_type
            };
        }
    }

    public class SendsignSendResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("message_sent")]
        public string? MessageSent { get; set; }
    }

    public class SendsignUpdateTTLRequest : SendsignRequest
    {
        [JsonPropertyName("time_to_live_hours")]
        public string? TTL { get; set; }
    }

    public class SendsignUpdateTTLResponse
    {
        [JsonPropertyName("ttl_updated")]
        public string? TTLUpdated { get; set; }
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
