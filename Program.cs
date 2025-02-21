var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Enable CORS to allow access from anywhere
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
             .AllowCredentials()
   );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true) // Enable Swagger in all environments
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS (Make sure this comes BEFORE Authorization)
app.UseCors("AllowAll");

app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.Run();