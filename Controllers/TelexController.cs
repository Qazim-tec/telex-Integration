using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

[Route("api/medalert")]
[ApiController]
public class TelexController : ControllerBase
{
    private readonly string _jsonFilePath = "data/integration-spec.json";
    private static string _reminderMessage = "Time for your medication!";
    private static string[] _alertRecipients = { "Patient" };
    private readonly WebhookService _webhookService;

    public TelexController(WebhookService webhookService)
    {
        _webhookService = webhookService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (!System.IO.File.Exists(_jsonFilePath))
            {
                Console.WriteLine("[Warning] JSON file not found. Using defaults.");
                return;
            }

            var jsonData = System.IO.File.ReadAllText(_jsonFilePath);
            using var document = JsonDocument.Parse(jsonData);
            var root = document.RootElement.GetProperty("data");

            var settingsArray = root.GetProperty("settings").EnumerateArray();
            foreach (var setting in settingsArray)
            {
                var label = setting.GetProperty("label").GetString();
                if (label == "Reminder Message")
                {
                    _reminderMessage = setting.GetProperty("default").GetString() ?? _reminderMessage;
                }
                else if (label == "Alert Recipients")
                {
                    if (setting.TryGetProperty("default", out var defaultValue) && defaultValue.ValueKind == JsonValueKind.String)
                    {
                        _alertRecipients = new[] { defaultValue.GetString() ?? "Patient" };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Failed to load settings: {ex.Message}");
        }
    }

    // Endpoint for Telex to fetch the integration spec (includes interval)
    [HttpGet("integration-spec")]
    public IActionResult GetIntegrationSpec()
    {
        if (!System.IO.File.Exists(_jsonFilePath))
            return NotFound(new { error = "Integration spec file not found" });

        var jsonData = System.IO.File.ReadAllText(_jsonFilePath);
        var jsonObject = JsonSerializer.Deserialize<object>(jsonData);

        return Ok(jsonObject);
    }

    // This endpoint is triggered automatically by Telex at the configured interval
    [HttpPost("tick")]
    public async Task<IActionResult> Tick([FromBody] MonitorPayload payload)
    {
        if (payload == null)
        {
            return BadRequest(new { error = "Invalid payload" });
        }

        // Fetch interval from the integration spec (e.g., from the JSON file)
        var interval = payload.GetSetting("Interval") ?? "* * * * *"; // Default to "* * * * *" (every minute)
        Console.WriteLine($"[Interval] {interval}");

        // Handle reminder logic based on received settings
        _reminderMessage = payload.GetSetting("Reminder Message") ?? _reminderMessage;
        _alertRecipients = payload.GetSetting("Alert Recipients")?.Split(",") ?? _alertRecipients;

        // Log channelId for debugging purposes
        Console.WriteLine($"[Received Tick] ChannelId: {payload.ChannelId}");

        // Log or return the returnUrl if needed
        Console.WriteLine($"[Return URL] {payload.ReturnUrl}");

        // Simulate sending a reminder after the interval (for illustration, add real interval logic here)
        await SendReminder();

        return Ok(new { success = true, message = "Reminder sent successfully", returnUrl = payload.ReturnUrl });
    }

    private async Task SendReminder()
    {
        string recipients = string.Join(", ", _alertRecipients);
        Console.WriteLine($"[Reminder Sent] {_reminderMessage} to {recipients}");

        // Send notification through webhook
        await _webhookService.SendWebhookNotification(
            "Medication Reminder",
            $"{_reminderMessage} to {recipients}"
        );
    }
}

public class MonitorPayload
{
    public string ChannelId { get; set; }
    public string ReturnUrl { get; set; }
    public List<Setting> Settings { get; set; }

    public string GetSetting(string label)
    {
        var setting = Settings?.Find(s => s.Label.Equals(label, StringComparison.OrdinalIgnoreCase));
        return setting?.Default;
    }
}

public class Setting
{
    public string Label { get; set; }
    public string Type { get; set; }
    public bool Required { get; set; }
    public string Default { get; set; }
}
