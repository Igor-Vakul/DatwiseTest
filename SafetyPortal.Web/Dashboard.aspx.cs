using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Newtonsoft.Json;
using SafetyPortal.Shared.Models;
using SafetyPortal.Web.Services;

namespace SafetyPortal.Web
{
    public partial class Dashboard : BasePage
    {
        protected DashboardStats Stats { get; private set; } = new DashboardStats();
        protected string CategoryJson { get; private set; } = "[]";
        protected string SeverityJson { get; private set; } = "[]";
        protected string StatusJson { get; private set; } = "[]";
        protected string DeptJson { get; private set; } = "[]";
        protected string TrendJson { get; private set; } = "[]";
        protected Dictionary<string, string> SeverityColors { get; private set; } = new Dictionary<string, string>();
        protected Dictionary<string, string> StatusColors { get; private set; } = new Dictionary<string, string>();

        // Prevents </script> from breaking out of a <script> block when JSON is inlined
        private static string SafeJson(string json)
            => json.Replace("</", "<\\/");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            try
            {
                Stats = new DashboardService(Token).GetDashboardStats() ?? new DashboardStats();

                var lookup = new LookupService(Token);
                SeverityColors = (lookup.GetSeverityLevels() ?? new List<SeverityLevelItem>())
                    .ToDictionary(s => s.Name, s => s.Color);
                StatusColors = (lookup.GetIncidentStatuses() ?? new List<IncidentStatusItem>())
                    .ToDictionary(s => s.Name, s => s.Color);
                var severityColors = SeverityColors;
                var statusColors = StatusColors;

                CategoryJson = SafeJson(JsonConvert.SerializeObject(
                    Stats.ByCategory.ConvertAll(x => new { label = x.Label, count = x.Count })));
                SeverityJson = SafeJson(JsonConvert.SerializeObject(
                    Stats.BySeverity.ConvertAll(x => new
                    {
                        label = x.Label,
                        count = x.Count,
                        color = severityColors.ContainsKey(x.Label) ? severityColors[x.Label] : "#6c757d"
                    })));
                StatusJson = SafeJson(JsonConvert.SerializeObject(
                    Stats.ByStatus.ConvertAll(x => new
                    {
                        label = x.Label,
                        count = x.Count,
                        color = statusColors.ContainsKey(x.Label) ? statusColors[x.Label] : "#6c757d"
                    })));
                DeptJson = SafeJson(JsonConvert.SerializeObject(
                    Stats.ByDepartment.ConvertAll(x => new { label = x.Label, count = x.Count, color = x.Color ?? "#6c757d" })));
                TrendJson = SafeJson(JsonConvert.SerializeObject(
                    Stats.ByMonth.ConvertAll(x => new { month = x.Month, count = x.Count })));
            }
            catch
            {
                // API not yet started — show empty dashboard
            }
        }
    }
}
