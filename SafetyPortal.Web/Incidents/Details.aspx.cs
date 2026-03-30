using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Incidents
{
    public partial class IncidentDetails : BasePage
    {
        protected System.Web.UI.WebControls.TextBox       txtActionTitle;
        protected System.Web.UI.WebControls.TextBox       txtActionDesc;
        protected System.Web.UI.WebControls.DropDownList  ddlActionUser;
        protected System.Web.UI.WebControls.TextBox       txtActionDue;
        protected System.Web.UI.WebControls.DropDownList  ddlActionPriority;
        protected System.Web.UI.WebControls.Button        btnAddAction;
        protected System.Web.UI.WebControls.FileUpload    fuDetailsAttachments;
        protected System.Web.UI.WebControls.Button        btnUpload;

        protected IncidentDetail       Incident    { get; private set; }
        protected List<AttachmentInfo> Attachments { get; private set; } = new List<AttachmentInfo>();
        protected string               ActionError { get; private set; } = string.Empty;
        protected string               UploadError { get; private set; } = string.Empty;

        private int IncidentId => int.TryParse(Request.QueryString["id"], out int id) ? id : 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (btnAddAction != null) btnAddAction.Text = T("save_action");
            if (btnUpload   != null) btnUpload.Text    = T("upload_btn");

            // Handle "complete action" GET action
            if (!IsPostBack)
            {
                var completeActionId = Request.QueryString["completeAction"];
                if (!string.IsNullOrEmpty(completeActionId) && int.TryParse(completeActionId, out int caId))
                {
                    Api.UpdateActionStatus(caId, "Completed");
                    Response.Redirect($"Details.aspx?id={IncidentId}", true);
                    return;
                }

                // Handle attachment delete
                var deleteAttId = Request.QueryString["deleteAttachment"];
                if (!string.IsNullOrEmpty(deleteAttId) && int.TryParse(deleteAttId, out int attId))
                {
                    Api.DeleteAttachment(IncidentId, attId);
                    Response.Redirect($"Details.aspx?id={IncidentId}", true);
                    return;
                }
            }

            Incident = Api.GetIncident(IncidentId);
            if (Incident != null)
                Attachments = Api.GetAttachments(IncidentId) ?? new List<AttachmentInfo>();

            if (!IsPostBack && Incident != null && IsManagerOrAdmin)
            {
                LoadUsers();
                txtActionDue.Text = DateTime.Today.AddDays(AppConstants.Validation.DefaultActionDueDays).ToString("yyyy-MM-dd");
            }
        }

        private void LoadUsers()
        {
            var users = Api.GetUsers() ?? new List<UserLookupItem>();
            ddlActionUser.Items.Clear();
            foreach (var u in users)
                ddlActionUser.Items.Add(new ListItem($"{u.FullName} [{u.RoleName}]", u.Id.ToString()));
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            Incident = Api.GetIncident(IncidentId);
            Attachments = Api.GetAttachments(IncidentId) ?? new List<AttachmentInfo>();

            if (!fuDetailsAttachments.HasFiles)
            {
                UploadError = T("attach_select_file");
                if (Incident != null && IsManagerOrAdmin) LoadUsers();
                return;
            }

            var errors = new StringBuilder();
            foreach (HttpPostedFile file in fuDetailsAttachments.PostedFiles)
            {
                if (file.ContentLength == 0) continue;

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    bytes = ms.ToArray();
                }

                var err = Api.UploadAttachment(
                    IncidentId, bytes,
                    Path.GetFileName(file.FileName),
                    file.ContentType);

                if (err != null)
                    errors.AppendLine($"• {HttpUtility.HtmlEncode(file.FileName)}: {HttpUtility.HtmlEncode(err)}");
            }

            if (errors.Length > 0)
            {
                UploadError = errors.ToString();
                if (Incident != null && IsManagerOrAdmin) LoadUsers();
                Attachments = Api.GetAttachments(IncidentId) ?? new List<AttachmentInfo>();
                return;
            }

            Response.Redirect($"Details.aspx?id={IncidentId}", true);
        }

        protected void btnAddAction_Click(object sender, EventArgs e)
        {
            Incident = Api.GetIncident(IncidentId);

            var title = txtActionTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                ActionError = "Action title is required.";
                LoadUsers();
                return;
            }

            if (!DateTime.TryParse(txtActionDue.Text, out var dueDate))
            {
                ActionError = "Invalid due date.";
                LoadUsers();
                return;
            }

            var req = new CreateCorrectiveActionRequest
            {
                ReportId          = IncidentId,
                ActionTitle       = title,
                ActionDescription = txtActionDesc.Text.Trim(),
                AssignedToUserId  = int.Parse(ddlActionUser.SelectedValue),
                DueDate           = dueDate.ToString("yyyy-MM-dd"),
                PriorityLevel     = ddlActionPriority.SelectedValue
            };

            if (Api.CreateCorrectiveAction(req))
                Response.Redirect($"Details.aspx?id={IncidentId}", true);
            else
            {
                ActionError = "Failed to add action.";
                LoadUsers();
            }
        }
    }
}
