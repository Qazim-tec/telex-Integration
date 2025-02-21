using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

[Route("api/medalert")]
[ApiController]
public class TelexController : ControllerBase
{
    private readonly string _jsonFilePath = "data/integration-spec.json";
    private static Timer? _reminderTimer;
    private static string _reminderMessage = "Time for your medication!";
    private static string _interval = "* * * * *"; // Default cron pattern
    private static string[] _alertRecipients = { "Patient" };
    private readonly WebhookService _webhookService;

    public TelexController(WebhookService webhookService)
    {
        _webhookService = webhookService;
        LoadSettings();
        StartReminderTimer();
    }

    private void LoadSettings()
    {
        if (System.IO.File.Exists(_jsonFilePath))
        {
            var jsonData = System.IO.File.ReadAllText(_jsonFilePath);
            using var document = JsonDocument.Parse(jsonData);
            var root = document.RootElement.GetProperty("data");

            // Extract settings from JSON
            _interval = root.GetProperty("settings")[0].GetProperty("default").GetString() ?? _interval;
            _reminderMessage = root.GetProperty("settings")[1].GetProperty("default").GetString() ?? _reminderMessage;

            // Extract recipients list
            var recipientsElement = root.GetProperty("settings")[2].GetProperty("default");
            _alertRecipients = new string[] { recipientsElement.GetString() ?? "Patient" };
        }
    }

    private void StartReminderTimer()
    {
        // Default interval: 1 minute (adjust as needed)
        int intervalMs = 60000;
        _reminderTimer = new Timer(SendReminder, null, intervalMs, intervalMs);
    }

    private async void SendReminder(object? state)
    {
        Console.WriteLine($"[Reminder Sent] {_reminderMessage} to {string.Join(", ", _alertRecipients)}");

        await _webhookService.SendWebhookNotification(
            "Medication Reminder",
            _reminderMessage,
            string.Join(", ", _alertRecipients)
        );
    }

    [HttpGet("integration-spec")]
    public IActionResult GetIntegrationSpec()
    {
        if (!System.IO.File.Exists(_jsonFilePath))
            return NotFound(new { error = "Integration spec file not found" });

        var jsonData = System.IO.File.ReadAllText(_jsonFilePath);
        var jsonObject = JsonSerializer.Deserialize<object>(jsonData);

        return Ok(jsonObject);
    }

    [HttpPost("tick")]
    public IActionResult Tick()
    {
        SendReminder(null);
        return Ok(new { success = true, message = "Reminder sent successfully" });
    }

    [HttpPost("collect")]
    public IActionResult Collect([FromBody] object payload)
    {
        Console.WriteLine($"[Data Received] {payload}");
        return Ok(new { success = true, message = "Data collected successfully" });
    }
}
