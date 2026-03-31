using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Dtos.Auth;
using SafetyPortal.Api.Entities;
using SafetyPortal.Api.Services;

namespace SafetyPortal.Api.Endpoints;

public static class AuthEndpoints
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequestDto request,
            SafetyPortalDbContext db,
            JwtTokenService jwtTokenService,
            ILogger<Program> logger,
            HttpContext http) =>
        {
            var ip = http.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var user = await db.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive);

            if (user is null)
            {
                logger.LogWarning("Failed login: unknown email {Email} from {IP}", request.Email, ip);
                db.AuditLogs.Add(new AuditLog
                {
                    EventType  = "LoginFailed",
                    UserEmail  = request.Email,
                    IpAddress  = ip,
                    Details    = "Unknown email or inactive account"
                });
                await db.SaveChangesAsync();
                return Results.Unauthorized();
            }

            // Check lockout
            if (user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
            {
                var remaining = (int)(user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes + 1;
                logger.LogWarning("Blocked login for locked account {Email} from {IP}", user.Email, ip);
                db.AuditLogs.Add(new AuditLog
                {
                    EventType  = "LoginBlocked",
                    UserEmail  = user.Email,
                    UserId     = user.Id,
                    IpAddress  = ip,
                    Details    = $"Account locked for {remaining} more minute(s)"
                });
                await db.SaveChangesAsync();
                return Results.Json(
                    new { error = $"Account locked. Try again in {remaining} minute(s)." },
                    statusCode: StatusCodes.Status423Locked);
            }

            var passwordHasher = new PasswordHasher<User>();
            var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);
                    logger.LogWarning(
                        "Account locked after {N} failed attempts: {Email} from {IP}",
                        user.FailedLoginAttempts, user.Email, ip);
                    db.AuditLogs.Add(new AuditLog
                    {
                        EventType  = "AccountLocked",
                        UserEmail  = user.Email,
                        UserId     = user.Id,
                        IpAddress  = ip,
                        Details    = $"Locked after {user.FailedLoginAttempts} failed attempts. Locked until {user.LockedUntil:u}"
                    });
                }
                else
                {
                    logger.LogWarning(
                        "Failed login attempt {N}/{Max} for {Email} from {IP}",
                        user.FailedLoginAttempts, MaxFailedAttempts, user.Email, ip);
                    db.AuditLogs.Add(new AuditLog
                    {
                        EventType  = "LoginFailed",
                        UserEmail  = user.Email,
                        UserId     = user.Id,
                        IpAddress  = ip,
                        Details    = $"Wrong password, attempt {user.FailedLoginAttempts}/{MaxFailedAttempts}"
                    });
                }

                await db.SaveChangesAsync();
                return Results.Unauthorized();
            }

            // Successful login — reset counter
            user.FailedLoginAttempts = 0;
            user.LockedUntil         = null;
            db.AuditLogs.Add(new AuditLog
            {
                EventType  = "LoginSuccess",
                UserEmail  = user.Email,
                UserId     = user.Id,
                IpAddress  = ip,
                Details    = $"Role: {user.Role.Name}"
            });
            await db.SaveChangesAsync();

            logger.LogInformation("Successful login: {Email} from {IP}", user.Email, ip);

            var token = jwtTokenService.CreateToken(user);

            return Results.Ok(new LoginResponseDto
            {
                AccessToken = token,
                UserId      = user.Id,
                FullName    = user.FullName,
                Email       = user.Email,
                Role        = user.Role.Name
            });
        })
        .RequireRateLimiting("login");

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                UserId   = user.FindFirstValue(ClaimTypes.NameIdentifier),
                FullName = user.FindFirstValue(ClaimTypes.Name),
                Email    = user.FindFirstValue(ClaimTypes.Email),
                Role     = user.FindFirstValue(ClaimTypes.Role)
            });
        })
        .RequireAuthorization();

        return app;
    }
}
