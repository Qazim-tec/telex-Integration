using Microsoft.AspNetCore.Mvc;
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
    private static int _intervalMinutes = 1; // Default interval

    public TelexController()
    {
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
            _reminderMessage = root.GetProperty("settings")[1].GetProperty("default").GetString() ?? _reminderMessage;
            _intervalMinutes = int.TryParse(root.GetProperty("settings")[0].GetProperty("default").GetString(), out int interval) ? interval : _intervalMinutes;
        }
    }

    private void StartReminderTimer()
    {
        _reminderTimer = new Timer(SendReminder, null, _intervalMinutes * 60000, _intervalMinutes * 60000);
    }

    private void SendReminder(object? state)
    {
        Console.WriteLine($"[Reminder] {_reminderMessage}");
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
        Console.WriteLine($"Received data: {payload}");
        return Ok(new { success = true, message = "Data collected successfully" });
    }
}
