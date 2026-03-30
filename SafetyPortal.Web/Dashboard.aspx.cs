using System;
using System.Web.UI;
using Newtonsoft.Json;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web
{
    public partial class Dashboard : BasePage
    {
        protected DashboardStats Stats       { get; private set; } = new DashboardStats();
        protected string         CategoryJson { get; private set; } = "[]";
        protected string         DeptJson     { get; private set; } = "[]";
        protected string         TrendJson    { get; private set; } = "[]";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            try
            {
                Stats = Api.GetDashboardStats() ?? new DashboardStats();

                CategoryJson = JsonConvert.SerializeObject(
                    Stats.ByCategory.ConvertAll(x => new { label = x.Label, count = x.Count }));
                DeptJson = JsonConvert.SerializeObject(
                    Stats.ByDepartment.ConvertAll(x => new { label = x.Label, count = x.Count }));
                TrendJson = JsonConvert.SerializeObject(
                    Stats.ByMonth.ConvertAll(x => new { month = x.Month, count = x.Count }));
            }
            catch
            {
                // API not yet started — show empty dashboard
            }
        }
    }
}
