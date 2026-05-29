using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Sendsign.JsonDocuments;

namespace Sendsign
{
    public class SendsignAPICaller
    {
        private readonly HttpClient client;
        public SendsignAPICaller(string host)
        {
            client = new HttpClient()
            {
                BaseAddress = new Uri(host)
            };
        }

        public static List<T> DeserializeSingleOrList<T>(string jsonReader)
        {
            List<T> examples = new List<T>();
            JsonNode elements = JsonNode.Parse(jsonReader) 
                ?? throw new JsonException("Unable to parse JSON");
            
            if (elements.GetValueKind() == JsonValueKind.Object)
            {
                examples.Add(JsonSerializer.Deserialize<T>(elements)!);
                return examples;
            } 
            else if (elements.GetValueKind() == JsonValueKind.Array) 
            {
                elements.AsArray().ToList().ForEach(element =>
                {
                    examples.Add(JsonSerializer.Deserialize<T>(element)!);
                });

                return examples;
            }

            throw new InvalidOperationException("Unexpected JSON input");
        }

        public static List<T> DeserializeSingleOrList<T>(Stream stream)
        {
            StreamReader streamReader = new StreamReader(stream);
            return DeserializeSingleOrList<T>(streamReader.ReadToEnd());
        }

        public SendsignSendResponse Send(
            string subject, 
            string body, 
            string customer_key, 
            string sender,
            string message_type,
            int? time_to_live_hours,
            IList<SendsignAttachment> attachments,
            IList<SendsignRecipient> recipients)
        {
            return Send(SendsignSendRequest.Create(
                subject,
                body,
                customer_key,
                sender,
                message_type,
                time_to_live_hours,
                attachments,
                recipients
            ));
        }

        public SendsignSendResponse Send(SendsignSendRequest request)
        {
            HttpContent message = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                }
            ));

            Task<HttpResponseMessage> httpResponse = client.PostAsync("/json/send", message);
            httpResponse.Wait();

            MediaTypeHeaderValue contentType = httpResponse.Result.Content.Headers.ContentType!;
            string content = contentType.ToString();

            SendsignSendResponse parsedResponse;
            if (content == "application/json")
            {
                parsedResponse = JsonSerializer.Deserialize<SendsignSendResponse>(httpResponse.Result.Content.ReadAsStream())
                    ?? throw new JsonException("Unable to parse incoming JSON");
            }
            else
            {
                Task<string> str = httpResponse.Result.Content.ReadAsStringAsync();
                str.Wait();
                parsedResponse = new SendsignSendResponse()
                {
                    Error = str.Result
                };
            }

            return parsedResponse;
        }

        public List<SendsignFetchResponse> Fetch(SendsignRequest request)
        {
            return Fetch([request]);
        }

        public List<SendsignFetchResponse> Fetch(List<SendsignRequest> requests)
        {
            HttpContent message = new StringContent(JsonSerializer.Serialize(
                requests,
                new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                }
            ));

            Task<HttpResponseMessage> httpResponseTask = 
                client.PostAsync("/json/fetch", message);
            httpResponseTask.Wait();
            HttpResponseMessage responseMessage = httpResponseTask.Result;

            MediaTypeHeaderValue contentType = 
                responseMessage.Content.Headers.ContentType!;
            
            string content = contentType.ToString();

            JsonNode parsedResponse;
            if (content == "application/json")
            {
                parsedResponse = JsonSerializer.Deserialize<JsonNode>(
                    httpResponseTask.Result.Content.ReadAsStream()) 
                    ?? throw new JsonException("Unable to parse incoming JSON");
            }
            else
            {
                Task<string> str = httpResponseTask.Result.Content.ReadAsStringAsync();
                str.Wait();
                throw new JsonException("Unable to fetch content: " + str.Result);
            }

            return DeserializeSingleOrList<SendsignFetchResponse>(
                JsonSerializer.Serialize(parsedResponse));
        }

        public SendsignCancelResponse CancelSignRequest(SendsignRequest request)
        {
            HttpContent message = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                }
            ));

            Task<HttpResponseMessage> httpResponseTask = 
                client.PostAsync("/json/cancel_sign_request", message);
            httpResponseTask.Wait();

            return JsonSerializer.Deserialize<SendsignCancelResponse>(
                httpResponseTask.Result.Content.ReadAsStream())
                ?? throw new JsonException("Unable to parse incoming JSON");
        }

        public List<SendsignRemindSignerResponse> SendsignRemindSigner(List<SendsignRequest> request)
        {
            HttpContent message = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                }
            ));

            Task<HttpResponseMessage> httpResponseTask = 
                client.PostAsync("/json/remind_signer", message);
            httpResponseTask.Wait();

            Task<string> data = httpResponseTask.Result.Content.ReadAsStringAsync();
            data.Wait();

            try
            {
                return JsonSerializer.Deserialize<List<SendsignRemindSignerResponse>>(data.Result) 
                    ?? throw new JsonException("Unable to parse incoming JSON");
            }
            catch
            {
                Console.WriteLine(data.Result);
                return [];
            }
        }

        public SendsignUpdateTTLResponse UpdateTTLForMessage(SendsignUpdateTTLRequest request)
        {
            HttpContent message = new StringContent(JsonSerializer.Serialize(request, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                }
            ));

            Task<HttpResponseMessage> httpResponseTask = 
                client.PostAsync("/json/update_ttl_for_message", message);
            httpResponseTask.Wait();
    
            return JsonSerializer.Deserialize<SendsignUpdateTTLResponse>(
                httpResponseTask.Result.Content.ReadAsStream()) 
                ?? throw new JsonException("Unable to parse incoming JSON");
        }
    }
}