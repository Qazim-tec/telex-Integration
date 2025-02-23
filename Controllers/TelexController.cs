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
    private readonly MedAlertService _medAlertService; // ✅ Add this


    public TelexController(WebhookService webhookService, MedAlertService medAlertService)
    {
        _webhookService = webhookService;
        _medAlertService = medAlertService; // ✅ Assign it
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

        Console.WriteLine($"[Tick Received] ChannelId: {payload.ChannelId}");
        Console.WriteLine($"[Return URL] {payload.ReturnUrl}");

        foreach (var setting in payload.Settings)
        {
            Console.WriteLine($"[Setting] {setting.Label}: {setting.Default}");
        }

        await _medAlertService.TriggerTick(payload); // ✅ Pass payload to use dynamic settings

        return Ok(new
        {
            success = true,
            message = "Reminder sent successfully",
            returnUrl = payload.ReturnUrl,
            receivedSettings = payload.Settings
        });
    }





}


