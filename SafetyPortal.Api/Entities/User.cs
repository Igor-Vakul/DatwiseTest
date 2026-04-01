namespace SafetyPortal.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // ── Account lockout ────────────────────────────────────────────────────
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<IncidentReport> ReportedIncidents { get; set; } = new List<IncidentReport>();
    public ICollection<IncidentReport> AssignedIncidents { get; set; } = new List<IncidentReport>();
    public ICollection<CorrectiveAction> AssignedCorrectiveActions { get; set; } = new List<CorrectiveAction>();
}
