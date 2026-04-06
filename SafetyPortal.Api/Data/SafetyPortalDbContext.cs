using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Entities;
using static SafetyPortal.Api.AppConstants;

namespace SafetyPortal.Api.Data;

public class SafetyPortalDbContext : DbContext
{
    public SafetyPortalDbContext(DbContextOptions<SafetyPortalDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<IncidentCategory> IncidentCategories => Set<IncidentCategory>();
    public DbSet<IncidentReport> IncidentReports => Set<IncidentReport>();
    public DbSet<CorrectiveAction> CorrectiveActions => Set<CorrectiveAction>();
    public DbSet<IncidentAttachment> IncidentAttachments => Set<IncidentAttachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<IncidentStatusOption> IncidentStatusOptions => Set<IncidentStatusOption>();
    public DbSet<SeverityLevelOption> SeverityLevelOptions => Set<SeverityLevelOption>();
    public DbSet<ActionStatusOption> ActionStatusOptions => Set<ActionStatusOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.HasData(
                new Role { Id = 1, Name = RoleName.Admin.ToString() },
                new Role { Id = 2, Name = RoleName.SafetyManager.ToString() },
                new Role { Id = 3, Name = RoleName.Supervisor.ToString() },
                new Role { Id = 4, Name = RoleName.Employee.ToString() }
            );
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.FailedLoginAttempts)
                .HasDefaultValue(0);

            entity.Property(x => x.LockedUntil)
                .IsRequired(false);

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.LocationName)
                .HasMaxLength(100);

            entity.Property(x => x.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#6c757d");

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.HasData(
                new Department { Id = 1, Name = "Production", LocationName = "Plant A", IsActive = true },
                new Department { Id = 2, Name = "Warehouse", LocationName = "Plant A", IsActive = true },
                new Department { Id = 3, Name = "Maintenance", LocationName = "Plant B", IsActive = true },
                new Department { Id = 4, Name = "Logistics", LocationName = "Plant B", IsActive = true },
                new Department { Id = 5, Name = "Quality Assurance", LocationName = "HQ", IsActive = true }
            );
        });

        modelBuilder.Entity<IncidentCategory>(entity =>
        {
            entity.ToTable("IncidentCategories");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(255);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(x => x.Name)
                .IsUnique();

            entity.HasData(
                new IncidentCategory { Id = 1, Name = "Near Miss", Description = "Event that could have caused injury or damage but did not" },
                new IncidentCategory { Id = 2, Name = "Hazard", Description = "Identified workplace hazard" },
                new IncidentCategory { Id = 3, Name = "Unsafe Condition", Description = "Unsafe physical condition in workplace" },
                new IncidentCategory { Id = 4, Name = "Unsafe Act", Description = "Unsafe employee action or behavior" }
            );
        });

        modelBuilder.Entity<IncidentReport>(entity =>
        {
            entity.ToTable("IncidentReports");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReportNumber)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Description)
                .IsRequired();

            entity.Property(x => x.LocationDetails)
                .HasMaxLength(200);

            entity.Property(x => x.IsArchived)
                .HasDefaultValue(false);

            entity.HasIndex(x => x.ReportNumber)
                .IsUnique();

            entity.HasOne(x => x.Category)
                .WithMany(x => x.IncidentReports)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Department)
                .WithMany(x => x.IncidentReports)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ReportedByUser)
                .WithMany(x => x.ReportedIncidents)
                .HasForeignKey(x => x.ReportedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AssignedToUser)
                .WithMany(x => x.AssignedIncidents)
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StatusOption)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SeverityLevelOption)
                .WithMany()
                .HasForeignKey(x => x.SeverityLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CorrectiveAction>(entity =>
        {
            entity.ToTable("CorrectiveActions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActionTitle)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ActionDescription)
                .HasMaxLength(500);

            entity.Property(x => x.PriorityLevel)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(x => x.Report)
                .WithMany(x => x.CorrectiveActions)
                .HasForeignKey(x => x.ReportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.AssignedToUser)
                .WithMany(x => x.AssignedCorrectiveActions)
                .HasForeignKey(x => x.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StatusOption)
                .WithMany()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IncidentAttachment>(entity =>
        {
            entity.ToTable("IncidentAttachments");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FileCategory).HasMaxLength(20).IsRequired();

            entity.HasOne(x => x.IncidentReport)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.IncidentReportId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IncidentStatusOption>(entity =>
        {
            entity.ToTable("IncidentStatusOptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(7).HasDefaultValue("#6c757d");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasData(
                new IncidentStatusOption { Id = 1, Name = "Open",       IsClosing = false, Color = "#0d6efd", DisplayOrder = 1, IsActive = true, IsSystem = true },
                new IncidentStatusOption { Id = 2, Name = "InProgress", IsClosing = false, Color = "#6610f2", DisplayOrder = 2, IsActive = true, IsSystem = true },
                new IncidentStatusOption { Id = 3, Name = "Closed",     IsClosing = true,  Color = "#198754", DisplayOrder = 3, IsActive = true, IsSystem = true }
            );
        });

        modelBuilder.Entity<SeverityLevelOption>(entity =>
        {
            entity.ToTable("SeverityLevelOptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(7).HasDefaultValue("#6c757d");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasData(
                new SeverityLevelOption { Id = 1, Name = "Low",      Color = "#198754", DisplayOrder = 1, IsActive = true, IsSystem = true },
                new SeverityLevelOption { Id = 2, Name = "Medium",   Color = "#ffc107", DisplayOrder = 2, IsActive = true, IsSystem = true },
                new SeverityLevelOption { Id = 3, Name = "High",     Color = "#fd7e14", DisplayOrder = 3, IsActive = true, IsSystem = true },
                new SeverityLevelOption { Id = 4, Name = "Critical", Color = "#dc3545", DisplayOrder = 4, IsActive = true, IsSystem = true }
            );
        });

        modelBuilder.Entity<ActionStatusOption>(entity =>
        {
            entity.ToTable("ActionStatusOptions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(7).HasDefaultValue("#6c757d");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasData(
                new ActionStatusOption { Id = 1, Name = "Pending",    IsCompleted = false, Color = "#6c757d", DisplayOrder = 1, IsActive = true, IsSystem = true },
                new ActionStatusOption { Id = 2, Name = "InProgress", IsCompleted = false, Color = "#0d6efd", DisplayOrder = 2, IsActive = true, IsSystem = true },
                new ActionStatusOption { Id = 3, Name = "Completed",  IsCompleted = true,  Color = "#198754", DisplayOrder = 3, IsActive = true, IsSystem = true }
            );
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EventType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.UserEmail).HasMaxLength(150);
            entity.Property(x => x.IpAddress).HasMaxLength(45);
            entity.Property(x => x.Details).HasMaxLength(500);

            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => x.EventType);
        });
    }
}