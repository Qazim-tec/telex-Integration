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
    private string _reminderMessage = "Time for your medication!";
    private string[] _alertRecipients = { "Patient" };
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
            var jsonObject = JsonSerializer.Deserialize<JsonDocument>(jsonData);

            if (jsonObject == null) return;

            var root = jsonObject.RootElement.GetProperty("data");

            foreach (var setting in root.GetProperty("settings").EnumerateArray())
            {
                var label = setting.GetProperty("label").GetString();
                if (label == "Reminder Message")
                {
                    _reminderMessage = setting.GetProperty("default").GetString() ?? _reminderMessage;
                }
                else if (label == "Alert Recipients" && setting.TryGetProperty("default", out var defaultValue))
                {
                    _alertRecipients = defaultValue.ValueKind == JsonValueKind.String
                        ? new[] { defaultValue.GetString() ?? "Patient" }
                        : _alertRecipients;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Failed to load settings: {ex.Message}");
        }
    }

    [HttpGet("integration-spec")]
    public IActionResult GetIntegrationSpec()
    {
        if (!System.IO.File.Exists(_jsonFilePath))
            return NotFound(new { error = "Integration spec file not found" });

        var jsonData = System.IO.File.ReadAllText(_jsonFilePath);
        return Content(jsonData, "application/json");
    }

    [HttpPost("tick")]
    public async Task<IActionResult> Tick([FromBody] MonitorPayload payload)
    {
        if (payload == null)
            return BadRequest(new { error = "Invalid payload" });

        // Fetch settings from payload
        _reminderMessage = payload.GetSetting("Reminder Message") ?? _reminderMessage;
        _alertRecipients = payload.GetSetting("Alert Recipients")?.Split(",") ?? _alertRecipients;

        Console.WriteLine($"[Tick Received] ChannelId: {payload.ChannelId}");
        Console.WriteLine($"[Return URL] {payload.ReturnUrl}");
        Console.WriteLine($"[Reminder] {_reminderMessage} to {string.Join(", ", _alertRecipients)}");

        await SendReminder();
        return Ok(new { success = true, message = "Reminder sent successfully", returnUrl = payload.ReturnUrl });
    }

    private async Task SendReminder()
    {
        string recipients = string.Join(", ", _alertRecipients);
        Console.WriteLine($"[Reminder Sent] {_reminderMessage} to {recipients}");

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
