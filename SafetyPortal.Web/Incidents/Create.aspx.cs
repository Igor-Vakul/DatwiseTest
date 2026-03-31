using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using SafetyPortal.Web.Models;

namespace SafetyPortal.Web.Incidents
{
    public partial class IncidentCreate : BasePage
    {
        protected System.Web.UI.WebControls.TextBox       txtTitle;
        protected System.Web.UI.WebControls.TextBox       txtDescription;
        protected System.Web.UI.WebControls.DropDownList  ddlCategory;
        protected System.Web.UI.WebControls.DropDownList  ddlDept;
        protected System.Web.UI.WebControls.TextBox       txtDate;
        protected System.Web.UI.WebControls.DropDownList  ddlSeverity;
        protected System.Web.UI.WebControls.TextBox       txtLocation;
        protected System.Web.UI.WebControls.DropDownList  ddlAssign;
        protected System.Web.UI.WebControls.Button        btnSave;
        protected System.Web.UI.WebControls.FileUpload    fuAttachments;

        protected string ErrorMessage  { get; private set; } = string.Empty;
        protected string UploadErrors  { get; private set; } = string.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {
            btnSave.Text = T("submit_report");
            if (!IsPostBack)
            {
                txtDate.Text = DateTime.Today.ToString(AppConstants.Validation.ISODateFormat);
                LoadLookups();
            }
        }

        private void LoadLookups()
        {
            var cats  = Api.GetCategories()  ?? new List<CategoryItem>();
            var depts = Api.GetDepartments() ?? new List<DepartmentItem>();
            var users = Api.GetUsers()       ?? new List<UserLookupItem>();

            foreach (var c in cats)
                ddlCategory.Items.Add(new ListItem(c.Name, c.Id.ToString()));

            foreach (var d in depts)
                ddlDept.Items.Add(new ListItem($"{d.Name} ({d.LocationName})", d.Id.ToString()));

            ddlAssign.Items.Add(new ListItem("— Unassigned —", ""));
            foreach (var u in users)
                ddlAssign.Items.Add(new ListItem($"{u.FullName} [{u.RoleName}]", u.Id.ToString()));
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var title = StripHtml(txtTitle.Text.Trim());
            var desc  = StripHtml(txtDescription.Text.Trim());

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(desc))
            {
                ErrorMessage = "Title and Description are required.";
                return;
            }

            if (!DateTime.TryParse(txtDate.Text, out var date))
            {
                ErrorMessage = "Invalid incident date.";
                return;
            }

            int? assignedTo = int.TryParse(ddlAssign.SelectedValue, out int uid) ? uid : (int?)null;

            var req = new CreateIncidentRequest
            {
                Title            = title,
                Description      = desc,
                CategoryId       = int.Parse(ddlCategory.SelectedValue),
                DepartmentId     = int.Parse(ddlDept.SelectedValue),
                IncidentDate     = date.ToString(AppConstants.Validation.ISODateFormat),
                LocationDetails  = StripHtml(txtLocation.Text.Trim()),
                SeverityLevel    = ddlSeverity.SelectedValue,
                AssignedToUserId = assignedTo
            };

            var incidentId = Api.CreateIncidentForId(req);
            if (incidentId == null)
            {
                ErrorMessage = "Failed to submit report. Please try again.";
                return;
            }

            // Upload attachments (if any)
            if (fuAttachments.HasFiles)
            {
                var errors = new StringBuilder();
                foreach (HttpPostedFile file in fuAttachments.PostedFiles)
                {
                    if (file.ContentLength == 0) continue;

                    byte[] bytes;
                    using (var ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);
                        bytes = ms.ToArray();
                    }

                    var err = Api.UploadAttachment(
                        incidentId.Value, bytes,
                        Path.GetFileName(file.FileName),
                        file.ContentType);

                    if (err != null)
                        errors.AppendLine($"• {HttpUtility.HtmlEncode(file.FileName)}: {HttpUtility.HtmlEncode(err)}");
                }

                if (errors.Length > 0)
                {
                    UploadErrors = errors.ToString();
                    // Incident was created — stay on page to show upload errors,
                    // but do not block navigation; user can proceed manually
                    return;
                }
            }

            Response.Redirect($"~/Incidents/List.aspx?created=1", true);
        }
    }
}
