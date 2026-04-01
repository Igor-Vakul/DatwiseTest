using System.Text;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SafetyPortal.Api;
using static SafetyPortal.Api.AppConstants;
using SafetyPortal.Api.Auth;
using Microsoft.AspNetCore.Http.Features;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Endpoints;
using SafetyPortal.Api.Jobs;
using SafetyPortal.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Swagger ────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Rate limiting ──────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    // Login: max 10 attempts per IP per minute, then 429
    options.AddFixedWindowLimiter("login", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 10;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // General API: 300 req/min per IP (DoS guard)
    options.AddFixedWindowLimiter("api", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 300;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── DataAnnotations validation for Minimal API DTOs ───────────────────────
builder.Services.AddValidation();

// ── Form / file upload limits (max single file = 20 MB) ───────────────────
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = AppConstants.Attachments.MaxDocumentBytes;
});
builder.WebHost.ConfigureKestrel(k =>
    k.Limits.MaxRequestBodySize = AppConstants.Attachments.MaxDocumentBytes);

// ── Antiforgery (required so .DisableAntiforgery() works per-endpoint) ────
builder.Services.AddAntiforgery();

// ── Database ───────────────────────────────────────────────────────────────
builder.Services.AddDbContext<SafetyPortalDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT ────────────────────────────────────────────────────────────────────
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? throw new InvalidOperationException("JWT settings are missing");

builder.Services.AddScoped<JwtTokenService>();

// ── Email ──────────────────────────────────────────────────────────────────
builder.Services.Configure<SendGridOptions>(
    builder.Configuration.GetSection(SendGridOptions.SectionName));
builder.Services.AddScoped<IEmailService, SendGridEmailService>();

// ── Hangfire jobs (scoped so they get DB context + email service via DI) ──
builder.Services.AddScoped<IncidentNotificationJob>();
builder.Services.AddScoped<CorrectiveActionReminderJob>();
builder.Services.AddScoped<IncidentEscalationJob>();

// ── Hangfire ───────────────────────────────────────────────────────────────
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };
    });

// ── Authorization policies ────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(RoleName.Admin.ToString()));
    options.AddPolicy("SafetyManagerOrAdmin", p => p.RequireRole(RoleName.Admin.ToString(), RoleName.SafetyManager.ToString()));
    options.AddPolicy("SupervisorOrAbove", p => p.RequireRole(RoleName.Admin.ToString(), RoleName.SafetyManager.ToString(), RoleName.Supervisor.ToString()));
    options.AddPolicy("Authenticated", p => p.RequireAuthenticatedUser());
});

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policy =>
        policy.WithOrigins("https://localhost:44300", "http://localhost:56789")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// ── Migrate + seed ─────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SafetyPortalDbContext>();
    db.Database.Migrate();
    await DbSeeder.SeedAsync(db);
}

// ── Pipeline ───────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("WebApp");
app.UseRateLimiter();

// ── Security headers ───────────────────────────────────────────────────────
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    ctx.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    // CSP: API serves JSON only — block everything except same-origin
    ctx.Response.Headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";
    await next();
});
app.UseAuthentication();
app.UseAuthorization();

// ── Hangfire dashboard (Admin only, cookie-based auth) ─────────────────────
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAdminAuthorizationFilter(jwtOptions)],
    AppPath = null,
    DashboardTitle = "SafetyPortal — Jobs"
});

// ── Scenario 2: recurring daily reminder for due corrective actions ────────
RecurringJob.AddOrUpdate<CorrectiveActionReminderJob>(
    recurringJobId: "corrective-action-reminders",
    methodCall: job => job.SendRemindersAsync(),
    cronExpression: AppConstants.Jobs.ReminderCron);

// ── Endpoints ──────────────────────────────────────────────────────────────
app.MapGet("/", () => Results.Ok(new { message = "SafetyPortal API is running" }))
    .WithTags("System");

app.MapAuthEndpoints();
app.MapHangfireLoginEndpoints();
app.MapIncidentEndpoints();
app.MapCorrectiveActionEndpoints();
app.MapDashboardEndpoints();
app.MapLookupEndpoints();
app.MapUserManagementEndpoints();
app.MapAdminDepartmentEndpoints();
app.MapAdminCategoryEndpoints();
app.MapAttachmentEndpoints();

app.Run();
