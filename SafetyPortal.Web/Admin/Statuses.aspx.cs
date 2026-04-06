using System;
using System.Collections.Generic;
using SafetyPortal.Shared.Models;
using SafetyPortal.Web.Services;

namespace SafetyPortal.Web.Admin
{
    public partial class StatusesAdmin : AdminPage
    {
        protected System.Web.UI.WebControls.TextBox txtName;
        protected System.Web.UI.WebControls.HiddenField hfType;
        protected System.Web.UI.WebControls.HiddenField hfId;
        protected System.Web.UI.WebControls.HiddenField hfFlag;
        protected System.Web.UI.WebControls.HiddenField hfColor;
        protected System.Web.UI.WebControls.HiddenField hfActive;
        protected System.Web.UI.WebControls.HiddenField hfDeleteId;
        protected System.Web.UI.WebControls.HiddenField hfDeleteType;
        protected System.Web.UI.WebControls.Button btnSave;
        protected System.Web.UI.WebControls.Button btnDelete;

        protected List<IncidentStatusItem> IncidentStatuses { get; private set; } = new List<IncidentStatusItem>();
        protected List<SeverityLevelItem> SeverityLevels { get; private set; } = new List<SeverityLevelItem>();
        protected List<ActionStatusItem> ActionStatuses { get; private set; } = new List<ActionStatusItem>();
        protected string Message { get; private set; } = string.Empty;
        protected string MessageType { get; private set; } = "info";

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadAll();
        }

        private void LoadAll()
        {
            var svc = new AdminService(Token);
            IncidentStatuses = svc.GetAllIncidentStatuses() ?? new List<IncidentStatusItem>();
            SeverityLevels = svc.GetAllSeverityLevels() ?? new List<SeverityLevelItem>();
            ActionStatuses = svc.GetAllActionStatuses() ?? new List<ActionStatusItem>();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var name = StripHtml(txtName.Text.Trim());
            if (string.IsNullOrEmpty(name))
            {
                Message = "Name is required.";
                MessageType = "danger";
                LoadAll();
                return;
            }

            var type = hfType.Value;
            var id = int.Parse(hfId.Value);
            bool flag = string.Equals(hfFlag.Value, "true", StringComparison.OrdinalIgnoreCase);
            bool isActive = string.Equals(hfActive.Value, "true", StringComparison.OrdinalIgnoreCase);
            var color = string.IsNullOrEmpty(hfColor.Value) ? "#6c757d" : hfColor.Value;
            var svc = new AdminService(Token);
            bool ok;

            switch (type)
            {
                case "incident":
                    ok = id == 0
                        ? svc.CreateIncidentStatus(name, flag, color)
                        : svc.UpdateIncidentStatus(id, name, flag, color, isActive);
                    break;
                case "severity":
                    ok = id == 0
                        ? svc.CreateSeverityLevel(name, color)
                        : svc.UpdateSeverityLevel(id, name, color, isActive);
                    break;
                case "action":
                    ok = id == 0
                        ? svc.CreateActionStatus(name, flag, color)
                        : svc.UpdateActionStatus(id, name, flag, color, isActive);
                    break;
                default:
                    ok = false;
                    break;
            }

            Message = ok ? "Saved successfully." : "Save failed. Name may already exist.";
            MessageType = ok ? "success" : "danger";

            if (ok)
            {
                hfId.Value = "0";
                txtName.Text = string.Empty;
            }

            LoadAll();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(hfDeleteId.Value, out int id) || id == 0) return;

            var type = hfDeleteType.Value;
            var svc = new AdminService(Token);
            bool ok;

            switch (type)
            {
                case "incident": ok = svc.DeleteIncidentStatus(id); break;
                case "severity": ok = svc.DeleteSeverityLevel(id); break;
                case "action":   ok = svc.DeleteActionStatus(id); break;
                default:         ok = false; break;
            }

            Message = ok ? "Deleted successfully." : "Cannot delete — status is in use or is a system status.";
            MessageType = ok ? "success" : "danger";
            LoadAll();
        }
    }
}
