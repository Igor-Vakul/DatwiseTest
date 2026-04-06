using System;
using System.Collections.Generic;
using SafetyPortal.Shared;
using SafetyPortal.Shared.Models;
using SafetyPortal.Web.Services;

namespace SafetyPortal.Web.CorrectiveActions
{
    public partial class CorrectiveActionList : BasePage
    {
        protected System.Web.UI.WebControls.DropDownList ddlStatus;
        protected System.Web.UI.WebControls.Button btnFilter;

        protected List<CorrectiveActionSummary> Actions { get; private set; } = new List<CorrectiveActionSummary>();
        protected string ExportQs { get; private set; } = string.Empty;
        protected System.Collections.Generic.Dictionary<string, string> ActionStatusColors { get; private set; } = new System.Collections.Generic.Dictionary<string, string>();

        protected void Page_Load(object sender, EventArgs e)
        {
            btnFilter.Text = Translate("filter");
            ddlStatus.Items[0].Text = Translate("all_statuses");

            // Handle "complete" GET param
            if (!IsPostBack)
            {
                var completeId = Request.QueryString["complete"];
                if (!string.IsNullOrEmpty(completeId) && int.TryParse(completeId, out int caId))
                {
                    new CorrectiveActionService(Token).UpdateActionStatus(caId, ActionStatus.Completed.ToString());
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

            Actions = new CorrectiveActionService(Token).GetCorrectiveActions(
                status: ddlStatus.SelectedValue
            ) ?? new List<CorrectiveActionSummary>();

            foreach (var s in new LookupService(Token).GetActionStatuses() ?? new List<ActionStatusItem>())
                ActionStatusColors[s.Name] = s.Color;
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
