using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Cronos;

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
                if (label == "Interval")
                {
                    _interval = setting.GetProperty("default").GetString() ?? _interval;
                }
                else if (label == "Reminder Message")
                {
                    _reminderMessage = setting.GetProperty("default").GetString() ?? _reminderMessage;
                }
                else if (label == "Alert Recipients")
                {
                    // Ensure we properly handle recipients
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

    private void StartReminderTimer()
    {
        try
        {
            var cronExpression = CronExpression.Parse(_interval);
            ScheduleNextRun(cronExpression);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Invalid cron expression '{_interval}': {ex.Message}");
        }
    }

    private void ScheduleNextRun(CronExpression cronExpression)
    {
        var now = DateTime.UtcNow;
        var nextOccurrence = cronExpression.GetNextOccurrence(now, TimeZoneInfo.Utc);

        if (nextOccurrence.HasValue)
        {
            var delay = (int)(nextOccurrence.Value - now).TotalMilliseconds;
            if (delay < 0) delay = 1000; // Ensure at least 1 second delay to prevent issues

            _reminderTimer?.Dispose();
            _reminderTimer = new Timer(async _ =>
            {
                await SendReminder();
                ScheduleNextRun(cronExpression); // Reschedule
            }, null, delay, Timeout.Infinite);
        }
    }

    private void SendReminderCallback(object? state)
    {
        _ = SendReminder();
        StartReminderTimer(); // Reschedule next run
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
    public async Task<IActionResult> Tick()
    {
        await SendReminder();
        return Ok(new { success = true, message = "Reminder sent successfully" });
    }

    [HttpPost("collect")]
    public IActionResult Collect([FromBody] object payload)
    {
        Console.WriteLine($"[Data Received] {payload}");
        return Ok(new { success = true, message = "Data collected successfully" });
    }
}
