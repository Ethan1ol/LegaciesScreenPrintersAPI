// Legacies Screen Printers API: startup and service setup.
// Reference: Microsoft (2025) Handle errors in ASP.NET Core APIs - https://learn.microsoft.com/en-us/aspnet/core/web-api/handle-errors
// Reference: Microsoft (2025) Enable Cross-Origin Requests (CORS) in ASP.NET Core - https://learn.microsoft.com/en-us/aspnet/core/security/cors
// Reference: Microsoft (2025) Applying migrations (EF Core) - https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying
// Reference: Microsoft (2025) Configure JWT bearer authentication in ASP.NET Core - https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LegaciesPrintersApi.Data;
using LegaciesPrintersApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Unhandled exceptions become RFC 7807 "problem details" JSON instead of stack traces.
builder.Services.AddProblemDetails();

// Connection string comes from configuration: appsettings.Development.json locally.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

// CORS: only the website's own origin(s) may call the API from a browser.
const string WebsiteCorsPolicy = "Website";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(WebsiteCorsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ---------- Authentication: the API's own JWTs ----------

// The signing key comes from configuration and the API refuses to start without a strong one.
var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
jwt.Validate(builder.Environment.IsDevelopment());

builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<GoogleTokenValidator>();
builder.Services.AddScoped<UserService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep claim names as issued ("sub", "role", "name") instead of .NET's long URIs.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.SigningKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = AuthClaims.Name,
            RoleClaimType = AuthClaims.Role,
        };
        options.Events = new JwtBearerEvents
        {
            // Send a JSON reason with 401s so the app can show "session expired".
            OnChallenge = async context =>
            {
                context.HandleResponse();
                // No token at all -> "Please sign in".
                var expired = context.AuthenticateFailure is SecurityTokenExpiredException;
                var rejected = context.AuthenticateFailure != null;
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = expired ? "Your session has expired. Please sign in again."
                        : rejected ? "Your session is no longer valid. Please sign in again."
                        : "Please sign in.",
                    code = expired ? "token_expired" : rejected ? "token_invalid" : "unauthorized",
                });
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "You don't have permission to do that.",
                    code = "forbidden",
                });
            },
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------- Database: migrate and seed ----------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    logger.LogInformation("Applying database migrations...");
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, app.Configuration, logger);
    logger.LogInformation("Database ready. CORS allowed origins: {Origins}",
        allowedOrigins.Length == 0 ? "(none)" : string.Join(", ", allowedOrigins));
}

// ---------- HTTP pipeline ----------

// Hosts like Azure end HTTPS at a proxy, so trust its forwarded headers.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownIPNetworks = { }, KnownProxies = { }, // the host's proxy address isn't fixed
});

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    // Outside development: send http:// callers to https:// and tell browsers to use HTTPS only.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Legacies Printers API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors(WebsiteCorsPolicy);
app.UseAuthentication(); // who is calling (reads the Bearer token)
app.UseAuthorization();  // may they do this ([Authorize] attributes)
app.MapControllers();

// Simple health check for the host (also wakes up a sleeping free tier).
app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.Run();
