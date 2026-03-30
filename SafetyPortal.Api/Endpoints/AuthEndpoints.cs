using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Dtos.Auth;
using SafetyPortal.Api.Entities;
using SafetyPortal.Api.Services;

namespace SafetyPortal.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequestDto request,
            SafetyPortalDbContext db,
            JwtTokenService jwtTokenService) =>
        {
            var user = await db.Users
                .Include(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive);

            if (user is null)
                return Results.Unauthorized();

            var passwordHasher = new PasswordHasher<User>();
            var verifyResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verifyResult == PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            var token = jwtTokenService.CreateToken(user);

            return Results.Ok(new LoginResponseDto
            {
                AccessToken = token,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.Name
            });
        });

        group.MapGet("/me", (ClaimsPrincipal user) =>
        {
            return Results.Ok(new
            {
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
                FullName = user.FindFirstValue(ClaimTypes.Name),
                Email = user.FindFirstValue(ClaimTypes.Email),
                Role = user.FindFirstValue(ClaimTypes.Role)
            });
        })
        .RequireAuthorization();

        return app;
    }
}
