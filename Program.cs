using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LunchSystem.Data;
using LunchSystem.Services;
using LunchSystem.Middleware;
using Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// 🔧 CONFIGURATION BUILDER: carrega appsettings + ENV VARS (Render)
// ------------------------------------------------------------
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables(); // <- Render injeta ORIGIN aqui

// ------------------------------------------------------------
// 📦 DATABASE
// ------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------------------------------------------------
// 🔐 JWT
// ------------------------------------------------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

builder.Services.AddAuthorization();

// ------------------------------------------------------------
// 🧰 HANGFIRE
// ------------------------------------------------------------
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

// ------------------------------------------------------------
// 🧩 SERVICES
// ------------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IReportService, ReportService>();

// ------------------------------------------------------------
// 🌐 CORS (puxando ORIGIN do Render)
// ------------------------------------------------------------

// ORIGIN vinda do painel do Render (Environment)
var renderOrigin = builder.Configuration["ORIGIN"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Se existir ORIGIN no Render, usa ela; senão usa o appsettings
        var fallbackOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!;
        var origins = !string.IsNullOrWhiteSpace(renderOrigin)
            ? new[] { renderOrigin }
            : fallbackOrigins;

        policy.WithOrigins(origins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ------------------------------------------------------------
// 🧱 MVC / API / SWAGGER
// ------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ------------------------------------------------------------
// 🚀 APP
// ------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TenantMiddleware>();

app.MapControllers();
app.MapHangfireDashboard();

// ------------------------------------------------------------
// ⏰ JOBS
// ------------------------------------------------------------
RecurringJob.AddOrUpdate<IReportService>(
    "generate-daily-reports",
    service => service.GenerateDailyReportsAsync(),
    "0 10 * * *",
    new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time")
    });

// ------------------------------------------------------------
// ❤️ HEALTHCHECK
// ------------------------------------------------------------
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// ------------------------------------------------------------
// ▶ RUN
// ------------------------------------------------------------
app.Run("http://0.0.0.0:5000");
