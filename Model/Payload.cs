using System.Text.Json.Serialization;

  

public class MonitorPayload
    {
        [JsonPropertyName("channel_id")] // ✅ Match the expected snake_case
        public string ChannelId { get; set; }

        [JsonPropertyName("return_url")]
        public string ReturnUrl { get; set; }

        [JsonPropertyName("settings")]
        public List<Setting> Settings { get; set; }
    }

    public class Setting
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("default")]
        public string Default { get; set; }
    }



