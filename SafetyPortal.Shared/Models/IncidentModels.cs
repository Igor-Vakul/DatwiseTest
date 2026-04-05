using System;
using System.Collections.Generic;

namespace SafetyPortal.Shared.Models
{
    public class IncidentSummary
    {
        public int Id { get; set; }
        public string ReportNumber { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string DepartmentName { get; set; }
        public string ReportedByFullName { get; set; }
        public DateTime IncidentDate { get; set; }
        public string SeverityLevel { get; set; }
        public string Status { get; set; }
        public int CorrectiveActionsCount { get; set; }
        public int AttachmentsCount { get; set; }
        public bool IsArchived { get; set; }
    }

    public class IncidentDetail
    {
        public int Id { get; set; }
        public string ReportNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int ReportedByUserId { get; set; }
        public string ReportedByFullName { get; set; }
        public int? AssignedToUserId { get; set; }
        public string AssignedToFullName { get; set; }
        public DateTime IncidentDate { get; set; }
        public DateTime ReportedAt { get; set; }
        public string LocationDetails { get; set; }
        public string SeverityLevel { get; set; }
        public string Status { get; set; }
        public bool IsArchived { get; set; }
        public List<CorrectiveActionSummary> CorrectiveActions { get; set; } = new List<CorrectiveActionSummary>();
    }

    public class CreateIncidentResult
    {
        public int Id { get; set; }
        public string ReportNumber { get; set; }
    }

    public class AttachmentInfo
    {
        public int Id { get; set; }
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
        public long FileSizeBytes { get; set; }
        public string FileCategory { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class CreateIncidentRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int DepartmentId { get; set; }
        public string IncidentDate { get; set; }
        public string LocationDetails { get; set; }
        public string SeverityLevel { get; set; }
        public int? AssignedToUserId { get; set; }
    }

    public class UpdateIncidentRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int DepartmentId { get; set; }
        public string IncidentDate { get; set; }
        public string LocationDetails { get; set; }
        public string SeverityLevel { get; set; }
        public string Status { get; set; }
        public int? AssignedToUserId { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
