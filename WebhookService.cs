using System;
using System.Net.Http;
using System.Net.Http.Json;
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

    public async Task<bool> SendWebhookNotification(string eventName, string message, string username)
    {
        var payload = new
        {
            event_name = eventName,
            message = message,
            status = "success",
            username = username
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(_webhookUrl, payload);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[Webhook Error] HTTP {response.StatusCode}: {error}");
                return false;
            }

            Console.WriteLine("[Webhook Sent Successfully]");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Webhook Error] {ex.Message}");
            return false;
        }
    }
}
