using RapidPhotoFlow.Api.Endpoints;
using RapidPhotoFlow.Api.Extensions;
using RapidPhotoFlow.Application.DependencyInjection;
using RapidPhotoFlow.Infrastructure.DependencyInjection;
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "RapidPhotoFlow")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/rapidphotoflow-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting RapidPhotoFlow API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Configuration
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    var uploadPath = builder.Configuration["Storage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

    // Add services
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "RapidPhotoFlow API", Version = "v1" });
    });

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // Add Application & Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(connectionString, uploadPath);

    var app = builder.Build();

    // Apply migrations
    await app.UseDatabaseMigration();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "RapidPhotoFlow API v1");
        });
    }

    // Request logging
    app.UseSerilogRequestLogging();

    app.UseCors("AllowFrontend");

    // Map endpoints
    app.MapPhotosEndpoints();
    app.MapEventLogEndpoints();

    // Health check
    app.MapGet("/", () => Results.Ok(new { Status = "Healthy", Service = "RapidPhotoFlow API" }))
       .WithName("HealthCheck")
       .WithTags("Health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
