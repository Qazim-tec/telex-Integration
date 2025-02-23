using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class MedAlertScheduler : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(10);
    private readonly HttpClient _httpClient;
    private readonly ILogger<MedAlertScheduler> _logger;
    private readonly IConfiguration _configuration;

    public MedAlertScheduler(IServiceProvider serviceProvider, ILogger<MedAlertScheduler> logger, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _httpClient = new HttpClient();
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var channelId = _configuration["Telex:ChannelId"];
                var returnUrl = _configuration["Telex:ReturnUrl"];

                var payload = new
                {
                    channel_id = channelId,
                    return_url = returnUrl,
                    settings = new[]
                    {
                    new { label = "Interval", type = "text", required = true, @default = "*/10 * * * *" }
                }
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation("[Scheduler] Sending Tick request...");

                var response = await _httpClient.PostAsync("http://localhost:5000/api/medalert/tick", content, stoppingToken);

                _logger.LogInformation($"[Scheduler] Tick response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"[Scheduler] Error response: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Scheduler] Error sending tick: {ex.Message}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}