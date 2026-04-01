using System;
using System.Collections.Generic;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.CorrectiveActions
{
    public partial class CorrectiveActionList : BasePage
    {
        protected System.Web.UI.WebControls.DropDownList ddlStatus;
        protected System.Web.UI.WebControls.Button       btnFilter;

        protected List<CorrectiveActionSummary> Actions   { get; private set; } = new List<CorrectiveActionSummary>();
        protected string                         ExportQs  { get; private set; } = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            btnFilter.Text = T("filter");
            ddlStatus.Items[0].Text = T("all_statuses");

            // Handle "complete" GET param
            if (!IsPostBack)
            {
                var completeId = Request.QueryString["complete"];
                if (!string.IsNullOrEmpty(completeId) && int.TryParse(completeId, out int caId))
                {
                    Api.UpdateActionStatus(caId, ActionStatus.Completed.ToString());
                    Response.Redirect($"List.aspx", true);
                    return;
                }

                ddlStatus.SelectedValue = Request.QueryString["status"] ?? string.Empty;
            }

            LoadActions();
        }

        private void LoadActions()
        {
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                ExportQs = "status=" + System.Web.HttpUtility.UrlEncode(ddlStatus.SelectedValue);

            Actions = Api.GetCorrectiveActions(
                status: ddlStatus.SelectedValue
            ) ?? new List<CorrectiveActionSummary>();
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            LoadActions();
        }

        protected bool IsOverdue(CorrectiveActionSummary a)
        {
            if (a.Status == ActionStatus.Completed.ToString()) return false;
            return DateTime.TryParse(a.DueDate, out var d) && d < DateTime.Today;
        }
    }
}
