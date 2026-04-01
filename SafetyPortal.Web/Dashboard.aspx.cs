using System;
using System.Web.UI;
using Newtonsoft.Json;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web
{
    public partial class Dashboard : BasePage
    {
        protected DashboardStats Stats        { get; private set; } = new DashboardStats();
        protected string         CategoryJson { get; private set; } = "[]";
        protected string         SeverityJson { get; private set; } = "[]";
        protected string         StatusJson   { get; private set; } = "[]";
        protected string         DeptJson     { get; private set; } = "[]";
        protected string         TrendJson    { get; private set; } = "[]";

        // Prevents </script> from breaking out of a <script> block when JSON is inlined
        private static string SafeJson(string json)
            => json.Replace("</", "<\\/");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            try
            {
                Stats = Api.GetDashboardStats() ?? new DashboardStats();

                CategoryJson = SafeJson(JsonConvert.SerializeObject(
                    Stats.ByCategory.ConvertAll(x => new { label = x.Label, count = x.Count })));
                SeverityJson = SafeJson(JsonConvert.SerializeObject(
                    Stats.BySeverity.ConvertAll(x => new { label = x.Label, count = x.Count })));
                StatusJson = SafeJson(JsonConvert.SerializeObject(
                    Stats.ByStatus.ConvertAll(x => new { label = x.Label, count = x.Count })));
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
