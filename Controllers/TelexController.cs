using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Text.Json;

[Route("api/medalert")]
[ApiController]
public class TelexController : ControllerBase
{
    private readonly string _jsonFilePath = "data/integration-spec.json";

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
        Console.WriteLine("Telex Tick triggered! Processing reminders...");
        return Ok(new { success = true, message = "Reminder sent successfully" });
    }

    [HttpPost("collect")]
    public IActionResult Collect([FromBody] object payload)
    {
        Console.WriteLine($"Received data: {payload}");
        return Ok(new { success = true, message = "Data collected successfully" });
    }
}
