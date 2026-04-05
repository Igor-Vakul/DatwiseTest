namespace SafetyPortal.Shared
{
    public enum RoleName       { Admin = 1, SafetyManager = 2, Supervisor = 3, Employee = 4 }
    public enum IncidentStatus { Open = 1, InProgress = 2, Closed = 3 }
    public enum SeverityLevel  { Low = 1, Medium = 2, High = 3, Critical = 4 }
    public enum ActionStatus   { Pending = 1, InProgress = 2, Completed = 3 }
    public enum TextDirection  { Ltr = 1, Rtl = 2 }
}
