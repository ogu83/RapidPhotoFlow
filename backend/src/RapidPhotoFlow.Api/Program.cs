using RapidPhotoFlow.Api.Endpoints;
using RapidPhotoFlow.Api.Extensions;
using RapidPhotoFlow.Application.DependencyInjection;
using RapidPhotoFlow.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

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

app.UseCors("AllowFrontend");

// Map endpoints
app.MapPhotosEndpoints();
app.MapEventLogEndpoints();

// Health check
app.MapGet("/", () => Results.Ok(new { Status = "Healthy", Service = "RapidPhotoFlow API" }))
   .WithName("HealthCheck")
   .WithTags("Health");

app.Run();
