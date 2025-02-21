using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS policies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowTelex", policy =>
       policy.AllowAnyMethod()
             .AllowAnyHeader()
             .WithOrigins(
                 "https://telex.im",
                 "https://*.telex.im"
             )
   );
});

// Register HttpClient for making HTTP requests
builder.Services.AddHttpClient();

// Register WebhookService
builder.Services.AddScoped<WebhookService>();

var app = builder.Build();

// Enable Swagger UI for API testing
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

// Enable CORS (should be before Authorization)
app.UseCors("AllowAll");

app.UseAuthorization();
app.UseStaticFiles();

// Map API controllers
app.MapControllers();

app.Run();
