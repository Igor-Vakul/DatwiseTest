using System;
using System.Collections.Generic;

namespace SafetyPortal.Web.Models
{
    // ── Auth ──────────────────────────────────────────────────────────────
    public class LoginRequest
    {
        public string Email    { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public int    UserId      { get; set; }
        public string FullName    { get; set; }
        public string Email       { get; set; }
        public string Role        { get; set; }
    }

    // ── Incidents ─────────────────────────────────────────────────────────
    public class IncidentSummary
    {
        public int      Id                     { get; set; }
        public string   ReportNumber           { get; set; }
        public string   Title                  { get; set; }
        public string   CategoryName           { get; set; }
        public string   DepartmentName         { get; set; }
        public string   ReportedByFullName     { get; set; }
        public DateTime IncidentDate           { get; set; }
        public string   SeverityLevel          { get; set; }
        public string   Status                 { get; set; }
        public int      CorrectiveActionsCount { get; set; }
    }

    public class IncidentDetail
    {
        public int      Id                 { get; set; }
        public string   ReportNumber       { get; set; }
        public string   Title              { get; set; }
        public string   Description        { get; set; }
        public int      CategoryId         { get; set; }
        public string   CategoryName       { get; set; }
        public int      DepartmentId       { get; set; }
        public string   DepartmentName     { get; set; }
        public int      ReportedByUserId   { get; set; }
        public string   ReportedByFullName { get; set; }
        public int?     AssignedToUserId   { get; set; }
        public string   AssignedToFullName { get; set; }
        public DateTime IncidentDate       { get; set; }
        public DateTime ReportedAt         { get; set; }
        public string   LocationDetails    { get; set; }
        public string   SeverityLevel      { get; set; }
        public string   Status             { get; set; }
        public List<CorrectiveActionSummary> CorrectiveActions { get; set; } = new List<CorrectiveActionSummary>();
    }

    public class CreateIncidentResult
    {
        public int    Id           { get; set; }
        public string ReportNumber { get; set; }
    }

    public class AttachmentInfo
    {
        public int      Id               { get; set; }
        public string   OriginalFileName { get; set; }
        public string   ContentType      { get; set; }
        public long     FileSizeBytes    { get; set; }
        public string   FileCategory     { get; set; }
        public DateTime UploadedAt       { get; set; }
    }

    public class CreateIncidentRequest
    {
        public string   Title            { get; set; }
        public string   Description      { get; set; }
        public int      CategoryId       { get; set; }
        public int      DepartmentId     { get; set; }
        public string   IncidentDate     { get; set; } // ISO 8601: yyyy-MM-dd
        public string   LocationDetails  { get; set; }
        public string   SeverityLevel    { get; set; }
        public int?     AssignedToUserId { get; set; }
    }

    public class UpdateIncidentRequest
    {
        public string   Title            { get; set; }
        public string   Description      { get; set; }
        public int      CategoryId       { get; set; }
        public int      DepartmentId     { get; set; }
        public string   IncidentDate     { get; set; } // ISO 8601: yyyy-MM-dd
        public string   LocationDetails  { get; set; }
        public string   SeverityLevel    { get; set; }
        public string   Status           { get; set; }
        public int?     AssignedToUserId { get; set; }
    }

    public class PagedResult<T>
    {
        public List<T> Items      { get; set; } = new List<T>();
        public int     TotalCount { get; set; }
        public int     Page       { get; set; }
        public int     PageSize   { get; set; }
    }

    // ── Corrective Actions ────────────────────────────────────────────────
    public class CorrectiveActionSummary
    {
        public int    Id                 { get; set; }
        public int    ReportId           { get; set; }
        public string ReportNumber       { get; set; }
        public string ActionTitle        { get; set; }
        public string AssignedToFullName { get; set; }
        public string DueDate            { get; set; }
        public string Status             { get; set; }
        public string PriorityLevel      { get; set; }
    }

    public class CreateCorrectiveActionRequest
    {
        public int    ReportId          { get; set; }
        public string ActionTitle       { get; set; }
        public string ActionDescription { get; set; }
        public int    AssignedToUserId  { get; set; }
        public string DueDate           { get; set; }
        public string PriorityLevel     { get; set; }
    }

    // ── Dashboard ─────────────────────────────────────────────────────────
    public class DashboardStats
    {
        public int TotalIncidents        { get; set; }
        public int OpenIncidents         { get; set; }
        public int ClosedIncidents       { get; set; }
        public int HighCriticalIncidents { get; set; }
        public int OverdueActions        { get; set; }
        public int PendingActions        { get; set; }
        public List<ChartItem>          ByCategory      { get; set; } = new List<ChartItem>();
        public List<ChartItem>          ByDepartment    { get; set; } = new List<ChartItem>();
        public List<MonthItem>          ByMonth         { get; set; } = new List<MonthItem>();
        public List<RecentIncidentItem> RecentIncidents { get; set; } = new List<RecentIncidentItem>();
    }

    public class ChartItem
    {
        public string CategoryName   { get; set; }
        public string DepartmentName { get; set; }
        public int    Count          { get; set; }
        public string Label          => CategoryName ?? DepartmentName;
    }

    public class MonthItem
    {
        public string Month { get; set; }
        public int    Count { get; set; }
    }

    public class RecentIncidentItem
    {
        public int      Id            { get; set; }
        public string   ReportNumber  { get; set; }
        public string   Title         { get; set; }
        public string   SeverityLevel { get; set; }
        public string   Status        { get; set; }
        public DateTime IncidentDate  { get; set; }
    }

    // ── Lookups ───────────────────────────────────────────────────────────
    public class DepartmentItem
    {
        public int    Id           { get; set; }
        public string Name         { get; set; }
        public string LocationName { get; set; }
    }

    public class CategoryItem
    {
        public int    Id          { get; set; }
        public string Name        { get; set; }
        public string Description { get; set; }
    }

    public class UserLookupItem
    {
        public int    Id       { get; set; }
        public string FullName { get; set; }
        public string Email    { get; set; }
        public string RoleName { get; set; }
    }

    public class RoleItem
    {
        public int    Id   { get; set; }
        public string Name { get; set; }
    }

    // ── Users (Admin) ─────────────────────────────────────────────────────
    public class UserSummary
    {
        public int    Id       { get; set; }
        public string FullName { get; set; }
        public string Email    { get; set; }
        public string RoleName { get; set; }
        public bool   IsActive { get; set; }
    }

    public class CreateUserRequest
    {
        public string FullName { get; set; }
        public string Email    { get; set; }
        public string Password { get; set; }
        public int    RoleId   { get; set; }
    }

    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public int    RoleId   { get; set; }
        public string Password { get; set; } // empty = keep existing password
    }
}
