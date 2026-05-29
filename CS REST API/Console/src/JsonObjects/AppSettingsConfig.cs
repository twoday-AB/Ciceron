using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

class AppSettings
{
    [JsonPropertyName("apiHost")]
    public string ApiHost { get; set; }
    
    [JsonPropertyName("provider")]
    public string Provider { get; set; }
    
    [JsonPropertyName("system")]
    public string System { get; set; }
    
    [JsonPropertyName("certificateIssuer")]
    public string CertificateIssuer { get; set; }

    private static readonly JsonSerializerOptions s_readOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static AppSettings LoadConfig(string path)
    {
        string text = File.ReadAllText(path);

        return JsonSerializer.Deserialize<AppSettings>(text, s_readOptions);
    }
}