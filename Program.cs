using LegaciesPrintersApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connect to SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Permissive CORS for Android phone and Web Dashboard
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Enable Swagger UI in all environments for testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Legacies Printers API v1");
    c.RoutePrefix = string.Empty; // Serves Swagger directly at the root URL
});

// IMPORTANT: Do NOT include app.UseHttpsRedirection() when running on HTTP

app.UseCors("AllowAll");
app.MapControllers();

app.Run("http://0.0.0.0:5000");