using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Entities;

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
    public DbSet<CorrectiveAction>    CorrectiveActions    => Set<CorrectiveAction>();
    public DbSet<IncidentAttachment> IncidentAttachments => Set<IncidentAttachment>();

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
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "SafetyManager" },
                new Role { Id = 3, Name = "Supervisor" },
                new Role { Id = 4, Name = "Employee" }
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

            entity.Property(x => x.SeverityLevel)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

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

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

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
    }
}