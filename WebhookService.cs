using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class WebhookService
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl = "https://ping.telex.im/v1/webhooks/0195262e-4f84-73be-8fed-e8b38b18da06";

    public WebhookService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendWebhookNotification(string eventName, string message, string username = "MedAlert")
    {
        var payload = new
        {
            event_name = eventName,
            message = message,
            status = "success",
            username = username
        };

        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(_webhookUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Webhook Error] {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Webhook Exception] {ex.Message}");
        }
    }
}
