using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SafetyPortal.Api.Data;

public class SafetyPortalDbContextFactory : IDesignTimeDbContextFactory<SafetyPortalDbContext>
{
    public SafetyPortalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SafetyPortalDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=SafetyPortalDb;Trusted_Connection=True;TrustServerCertificate=True");

        return new SafetyPortalDbContext(optionsBuilder.Options);
    }
}
