namespace SafetyPortal.Shared.Models
{
    public class CorrectiveActionSummary
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string ReportNumber { get; set; }
        public string ActionTitle { get; set; }
        public string AssignedToFullName { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
        public string PriorityLevel { get; set; }
    }

    public class CreateCorrectiveActionRequest
    {
        public int ReportId { get; set; }
        public string ActionTitle { get; set; }
        public string ActionDescription { get; set; }
        public int AssignedToUserId { get; set; }
        public string DueDate { get; set; }
        public string PriorityLevel { get; set; }
    }
}
