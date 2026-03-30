using System;
using System.Web.Script.Serialization;
using System.Web.UI;
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
                var jss = new JavaScriptSerializer();

                CategoryJson = jss.Serialize(
                    Stats.ByCategory.ConvertAll(x => new { label = x.Label, count = x.Count }));
                DeptJson = jss.Serialize(
                    Stats.ByDepartment.ConvertAll(x => new { label = x.Label, count = x.Count }));
                TrendJson = jss.Serialize(
                    Stats.ByMonth.ConvertAll(x => new { month = x.Month, count = x.Count }));
            }
            catch
            {
                // API not yet started — show empty dashboard
            }
        }
    }
}
