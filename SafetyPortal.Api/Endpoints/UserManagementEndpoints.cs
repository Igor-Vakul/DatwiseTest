using System.Security.Claims;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Dtos.Users;
using SafetyPortal.Api.Entities;
using SafetyPortal.Api.Services;

namespace SafetyPortal.Api.Endpoints;

public static class UserManagementEndpoints
{
    public static IEndpointRouteBuilder MapUserManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization("AdminOnly");

        // GET /api/users
        group.MapGet("/", async (SafetyPortalDbContext db) =>
        {
            var users = await db.Users
                .Include(x => x.Role)
                .OrderBy(x => x.FullName)
                .Select(x => new UserSummaryDto(x.Id, x.FullName, x.Email, x.Role.Name, x.IsActive))
                .ToListAsync();
            return Results.Ok(users);
        });

        // POST /api/users
        group.MapPost("/", async (CreateUserDto request, SafetyPortalDbContext db) =>
        {
            if (await db.Users.AnyAsync(x => x.Email == request.Email))
                return Results.Conflict(new { message = "Email already exists." });

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                RoleId = request.RoleId,
                IsActive = true
            };
            user.PasswordHash = hasher.HashPassword(user, request.Password);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/users/{user.Id}", new { user.Id });
        });

        // PUT /api/users/{id}
        group.MapPut("/{id:int}", async (int id, UpdateUserDto request, SafetyPortalDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
                return Results.NotFound();

            user.FullName = request.FullName;
            user.RoleId = request.RoleId;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, request.Password);
            }

            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // PUT /api/users/{id}/toggle-active
        group.MapPut("/{id:int}/toggle-active", async (int id, SafetyPortalDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
                return Results.NotFound();

            user.IsActive = !user.IsActive;
            await db.SaveChangesAsync();

            return Results.Ok(new { user.Id, user.IsActive });
        });

        // POST /api/users/{id}/send-email
        group.MapPost("/{id:int}/send-email", async (int id, SendEmailDto request, ClaimsPrincipal principal, SafetyPortalDbContext db) =>
        {
            if (int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out int callerId) && callerId == id)
                return Results.BadRequest(new { error = "You cannot send an email to yourself." });

            var user = await db.Users.FindAsync(id);
            if (user is null)
                return Results.NotFound();

            if (string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
                return Results.BadRequest(new { error = "Subject and body are required." });

            BackgroundJob.Enqueue<IEmailService>(s =>
                s.SendDirectAsync(user.Email, user.FullName, request.Subject, request.Body));

            return Results.Ok(new { queued = true });
        });

        return app;
    }
}

public record SendEmailDto(
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(200)] string Subject,
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.StringLength(5000)] string Body
);
