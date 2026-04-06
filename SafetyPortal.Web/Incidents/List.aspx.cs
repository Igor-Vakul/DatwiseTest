using System;
using System.Collections.Generic;
using System.Web.UI;
using SafetyPortal.Shared;
using SafetyPortal.Shared.Models;
using SafetyPortal.Web.Services;

namespace SafetyPortal.Web.Incidents
{
    public partial class IncidentList : BasePage
    {
        protected System.Web.UI.WebControls.TextBox txtSearch;
        protected System.Web.UI.WebControls.DropDownList ddlStatus;
        protected System.Web.UI.WebControls.DropDownList ddlSeverity;
        protected System.Web.UI.WebControls.DropDownList ddlDept;
        protected System.Web.UI.WebControls.DropDownList ddlCategory;
        protected System.Web.UI.WebControls.Button btnSearch;

        protected List<IncidentSummary> Incidents { get; private set; } = new List<IncidentSummary>();
        protected int TotalCount { get; private set; }
        protected int CurrentPage { get; private set; } = 1;
        protected int TotalPages { get; private set; } = 1;
        protected string FilterQs { get; private set; } = string.Empty;
        protected bool ShowArchived { get; private set; } = false;

        private const int PageSize = AppConstants.Pagination.DefaultPageSize;

        protected void Page_Load(object sender, EventArgs e)
        {
            btnSearch.Text = Translate("filter");
            ddlStatus.Items[0].Text = Translate("all_statuses");
            ddlSeverity.Items[0].Text = Translate("all_severities");

            if (!IsPostBack)
            {
                // Toggle archive via GET ?toggle_archive=id
                var toggleId = Request.QueryString["toggle_archive"];
                if (!string.IsNullOrEmpty(toggleId) && int.TryParse(toggleId, out int tid)
                    && IsManagerOrAdmin)
                {
                    new IncidentService(Token).ToggleIncidentArchive(tid);
                    // Redirect back preserving other filters but without the toggle param
                    var redirect = "List.aspx?" + (Request.QueryString["archived"] == "true"
                        ? "archived=true&" : "");
                    Response.Redirect(redirect, true);
                    return;
                }

                ShowArchived = Request.QueryString["archived"] == "true";
                CurrentPage = int.TryParse(Request.QueryString["page"], out int p) ? Math.Max(1, p) : 1;
                LoadLookups();
                RestoreFiltersFromQs();
                LoadIncidents();
            }
        }

        private void LoadLookups()
        {
            var lookup = new LookupService(Token);
            var depts = lookup.GetDepartments() ?? new List<DepartmentItem>();
            ddlDept.Items.Add(new System.Web.UI.WebControls.ListItem("All Departments", ""));
            foreach (var d in depts)
                ddlDept.Items.Add(new System.Web.UI.WebControls.ListItem(d.Name, d.Id.ToString()));

            var cats = lookup.GetCategories() ?? new List<CategoryItem>();
            ddlCategory.Items.Add(new System.Web.UI.WebControls.ListItem("All Categories", ""));
            foreach (var c in cats)
                ddlCategory.Items.Add(new System.Web.UI.WebControls.ListItem(c.Name, c.Id.ToString()));
        }

        private void RestoreFiltersFromQs()
        {
            txtSearch.Text = Request.QueryString["search"] ?? string.Empty;
            ddlStatus.SelectedValue = Request.QueryString["status"] ?? string.Empty;
            ddlSeverity.SelectedValue = Request.QueryString["severity"] ?? string.Empty;

            var deptVal = Request.QueryString["dept"] ?? string.Empty;
            var catVal = Request.QueryString["cat"] ?? string.Empty;
            if (ddlDept.Items.FindByValue(deptVal) != null) ddlDept.SelectedValue = deptVal;
            if (ddlCategory.Items.FindByValue(catVal) != null) ddlCategory.SelectedValue = catVal;
        }

        private void LoadIncidents()
        {
            int? deptId = int.TryParse(ddlDept.SelectedValue, out int d) ? d : (int?)null;
            int? catId = int.TryParse(ddlCategory.SelectedValue, out int c) ? c : (int?)null;

            FilterQs = BuildFilterQs();

            var result = new IncidentService(Token).GetIncidents(
                page: CurrentPage,
                pageSize: PageSize,
                search: txtSearch.Text.Trim(),
                status: ddlStatus.SelectedValue,
                severityLevel: ddlSeverity.SelectedValue,
                departmentId: deptId,
                categoryId: catId,
                archived: ShowArchived
            ) ?? new PagedResult<IncidentSummary>();

            Incidents = result.Items;
            TotalCount = result.TotalCount;
            TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
            if (TotalPages < 1) TotalPages = 1;
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ShowArchived = Request.QueryString["archived"] == "true";
            CurrentPage = 1;
            LoadIncidents();
        }

        private string BuildFilterQs()
        {
            var parts = new List<string>();
            if (ShowArchived) parts.Add("archived=true");
            if (!string.IsNullOrEmpty(txtSearch.Text)) parts.Add("search=" + System.Web.HttpUtility.UrlEncode(txtSearch.Text));
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue)) parts.Add("status=" + ddlStatus.SelectedValue);
            if (!string.IsNullOrEmpty(ddlSeverity.SelectedValue)) parts.Add("severity=" + ddlSeverity.SelectedValue);
            if (!string.IsNullOrEmpty(ddlDept.SelectedValue)) parts.Add("dept=" + ddlDept.SelectedValue);
            if (!string.IsNullOrEmpty(ddlCategory.SelectedValue)) parts.Add("cat=" + ddlCategory.SelectedValue);
            return string.Join("&", parts);
        }
    }
}
