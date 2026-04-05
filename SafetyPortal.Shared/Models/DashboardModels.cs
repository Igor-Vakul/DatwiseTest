using System;
using System.Collections.Generic;

namespace SafetyPortal.Shared.Models
{
    public class DashboardStats
    {
        public int TotalIncidents { get; set; }
        public int OpenIncidents { get; set; }
        public int ClosedIncidents { get; set; }
        public int HighCriticalIncidents { get; set; }
        public int OverdueActions { get; set; }
        public int PendingActions { get; set; }
        public List<ChartItem> ByCategory { get; set; } = new List<ChartItem>();
        public List<ChartItem> ByDepartment { get; set; } = new List<ChartItem>();
        public List<LabelItem> BySeverity { get; set; } = new List<LabelItem>();
        public List<LabelItem> ByStatus { get; set; } = new List<LabelItem>();
        public List<MonthItem> ByMonth { get; set; } = new List<MonthItem>();
        public List<RecentIncidentItem> RecentIncidents { get; set; } = new List<RecentIncidentItem>();
    }

    public class ChartItem
    {
        public string CategoryName { get; set; }
        public string DepartmentName { get; set; }
        public string Color { get; set; }
        public int Count { get; set; }
        public string Label => CategoryName ?? DepartmentName;
    }

    public class LabelItem
    {
        public string Label { get; set; }
        public int Count { get; set; }
    }

    public class MonthItem
    {
        public string Month { get; set; }
        public int Count { get; set; }
    }

    public class RecentIncidentItem
    {
        public int Id { get; set; }
        public string ReportNumber { get; set; }
        public string Title { get; set; }
        public string SeverityLevel { get; set; }
        public string Status { get; set; }
        public DateTime IncidentDate { get; set; }
        public int AttachmentsCount { get; set; }
    }
}
