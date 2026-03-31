namespace SafetyPortal.Api.Entities;

public class AuditLog
{
    public int      Id          { get; set; }
    public DateTime OccurredAt  { get; set; } = DateTime.UtcNow;
    public string   EventType   { get; set; } = string.Empty;  // e.g. "LoginFailed", "AccountLocked", "UserCreated"
    public string?  UserEmail   { get; set; }
    public int?     UserId      { get; set; }
    public string?  IpAddress   { get; set; }
    public string?  Details     { get; set; }
}
