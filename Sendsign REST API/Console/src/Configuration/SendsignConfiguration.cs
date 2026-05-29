using System.Text.Json;
using System.Text.Json.Serialization;

class SendsignConfiguration
{
    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("customer_key")]
    public string CustomerKey { get; set; }

    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    public static SendsignConfiguration Load(string filename)
    {
        return JsonSerializer.Deserialize<SendsignConfiguration>(File.ReadAllText(filename));
    }
}