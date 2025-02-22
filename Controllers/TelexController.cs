using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

[Route("api/medalert")]
[ApiController]
public class TelexController : ControllerBase
{
    private readonly string _jsonFilePath = "data/integration-spec.json";
    private string _reminderMessage = "Time for your medication!";
    private string[] _alertRecipients = { "Patient" };
    private readonly WebhookService _webhookService;
    private readonly ILogger<TelexController> _logger;

    public TelexController(WebhookService webhookService, ILogger<TelexController> logger)
    {
        _webhookService = webhookService;
        _logger = logger;
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            if (!System.IO.File.Exists(_jsonFilePath))
            {
                _logger.LogWarning("JSON file not found. Using default settings.");
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
            _logger.LogError($"Failed to load settings: {ex.Message}");
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
    public IActionResult Tick([FromBody] MonitorPayload payload)
    {
        if (payload == null)
        {
            return BadRequest(new { error = "Invalid payload" });
        }

        _logger.LogInformation("Received tick for ChannelId: {ChannelId}", payload.ChannelId);

        // Fetch settings
        string interval = payload.GetSetting("Interval") ?? "* * * * *";
        _reminderMessage = payload.GetSetting("Reminder Message") ?? _reminderMessage;
        _alertRecipients = payload.GetSetting("Alert Recipients")?.Split(",") ?? _alertRecipients;

        _logger.LogInformation("Reminder set for {Recipients}: {Message}", string.Join(", ", _alertRecipients), _reminderMessage);

        // Process reminder asynchronously
        Task.Run(() => SendReminder());

        return Accepted(new
        {
            success = true,
            message = "Reminder is being processed",
            returnUrl = payload.ReturnUrl
        });
    }

    private async Task SendReminder()
    {
        string recipients = string.Join(", ", _alertRecipients);
        _logger.LogInformation("Sending reminder: {Message} to {Recipients}", _reminderMessage, recipients);

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
        return Settings?.Find(s => s.Label.Equals(label, StringComparison.OrdinalIgnoreCase))?.Default;
    }
}

public class Setting
{
    public string Label { get; set; }
    public string Type { get; set; }
    public bool Required { get; set; }
    public string Default { get; set; }
}
