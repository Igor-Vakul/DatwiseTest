using System;
using System.Collections.Generic;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.CorrectiveActions
{
    public partial class CorrectiveActionList : BasePage
    {
        protected List<CorrectiveActionSummary> Actions { get; private set; } = new List<CorrectiveActionSummary>();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Handle "complete" GET param
            if (!IsPostBack)
            {
                var completeId = Request.QueryString["complete"];
                if (!string.IsNullOrEmpty(completeId) && int.TryParse(completeId, out int caId))
                {
                    Api.UpdateActionStatus(caId, "Completed");
                    Response.Redirect($"List.aspx", true);
                    return;
                }

                ddlStatus.SelectedValue = Request.QueryString["status"] ?? string.Empty;
            }

            LoadActions();
        }

        private void LoadActions()
        {
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
            if (a.Status == "Completed") return false;
            return DateTime.TryParse(a.DueDate, out var d) && d < DateTime.Today;
        }
    }
}
