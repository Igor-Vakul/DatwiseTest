using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Entities;

namespace SafetyPortal.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(SafetyPortalDbContext db)
    {
        if (await db.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<User>();

        var admin = new User
        {
            FullName = "System Admin",
            Email = "admin@datwise.local",
            RoleId = AppConstants.Roles.AdminId,
            IsActive = true
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        var safetyManager = new User
        {
            FullName = "Igor Vakul",
            Email = "safety.manager@datwise.local",
            RoleId = AppConstants.Roles.SafetyManagerId,
            IsActive = true
        };
        safetyManager.PasswordHash = hasher.HashPassword(safetyManager, "Safety123!");

        await db.Users.AddRangeAsync(admin, safetyManager);
        await db.SaveChangesAsync();
    }
}
