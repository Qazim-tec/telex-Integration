var builder = WebApplication.CreateBuilder(args);

try
{
    // Your service registrations here...
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        });

        options.AddPolicy("AllowTelex", policy =>
            policy.AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithOrigins("https://telex.im", "https://*.telex.im")
        );
    });

    builder.Services.AddHttpClient();
    builder.Services.AddScoped<WebhookService>();
    builder.Services.AddScoped<MedAlertService>(); // ✅ Register it
    builder.Services.AddHostedService<MedAlertScheduler>();
    

    var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthorization();
    app.UseStaticFiles();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"[Startup Error] {ex.Message}");
    throw;
}
