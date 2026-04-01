<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Details.aspx.cs"
         Inherits="SafetyPortal.Web.Incidents.IncidentDetails" MasterPageFile="~/Site.Master"
         ValidateRequest="false" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= T("incident_details") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-file-text me-2 text-primary"></i><%= T("incident_details") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

<% if (Incident == null) { %>
    <div class="alert alert-warning"><%= T("incident_not_found") %></div>
<% } else { %>

    <nav aria-label="breadcrumb" class="mb-3">
        <ol class="breadcrumb">
            <li class="breadcrumb-item"><a href="<%= ResolveUrl("~/Incidents/List.aspx") %>"><%= T("nav_incidents") %></a></li>
            <li class="breadcrumb-item active"><code><%= Incident.ReportNumber %></code></li>
        </ol>
    </nav>

    <div class="row g-3 mb-3">
        <div class="col-lg-8">
            <div class="card sp-card h-100">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <span><i class="bi bi-info-circle text-primary"></i> <%= System.Web.HttpUtility.HtmlEncode(Incident.Title) %></span>
                    <div class="d-flex gap-2">
                        <span class="badge badge-severity-<%= Incident.SeverityLevel.ToLower() %> fs-6"><%= Incident.SeverityLevel %></span>
                        <span class="badge badge-status-<%= Incident.Status.ToLower() %> fs-6"><%= Incident.Status %></span>
                    </div>
                </div>
                <div class="card-body">
                    <p class="text-muted"><%= System.Web.HttpUtility.HtmlEncode(Incident.Description) %></p>
                    <div class="row g-3 mt-1">
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-tag text-muted"></i>
                                <div><small class="text-muted d-block"><%= T("category") %></small><strong><%= System.Web.HttpUtility.HtmlEncode(Incident.CategoryName) %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-building text-muted"></i>
                                <div><small class="text-muted d-block"><%= T("department") %></small><strong><%= System.Web.HttpUtility.HtmlEncode(Incident.DepartmentName) %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-geo-alt text-muted"></i>
                                <div><small class="text-muted d-block"><%= T("location") %></small><strong><%= System.Web.HttpUtility.HtmlEncode(Incident.LocationDetails ?? "—") %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-calendar-event text-muted"></i>
                                <div><small class="text-muted d-block"><%= T("incident_date") %></small><strong><%= Incident.IncidentDate.ToString("dd MMMM yyyy") %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-person text-muted"></i>
                                <div><small class="text-muted d-block"><%= T("reported_by") %></small><strong><%= System.Web.HttpUtility.HtmlEncode(Incident.ReportedByFullName) %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-person-check text-muted"></i>
                                <div><small class="text-muted d-block"><%= T("assigned_to") %></small><strong><%= System.Web.HttpUtility.HtmlEncode(Incident.AssignedToFullName ?? T("unassigned")) %></strong></div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="card-footer d-flex gap-2">
                    <a href="<%= ResolveUrl("~/Incidents/Edit.aspx?id=" + Incident.Id) %>"
                       class="btn btn-outline-primary btn-sm">
                        <i class="bi bi-pencil me-1"></i><%= T("edit") %>
                    </a>
                    <a href="<%= ResolveUrl("~/Incidents/List.aspx") %>"
                       class="btn btn-outline-secondary btn-sm">
                        <i class="bi bi-arrow-left me-1"></i><%= T("back_to_list") %>
                    </a>
                </div>
            </div>
        </div>

        <div class="col-lg-4">
            <div class="card sp-card">
                <div class="card-header"><i class="bi bi-info-square text-secondary"></i> <%= T("report_info") %></div>
                <div class="card-body">
                    <dl class="row mb-0 small">
                        <dt class="col-5 text-muted"><%= T("report_number") %></dt>
                        <dd class="col-7"><code><%= Incident.ReportNumber %></code></dd>
                        <dt class="col-5 text-muted"><%= T("reported_at") %></dt>
                        <dd class="col-7"><%= Incident.ReportedAt.ToString("dd MMM yyyy HH:mm") %></dd>
                        <dt class="col-5 text-muted"><%= T("severity") %></dt>
                        <dd class="col-7"><span class="badge badge-severity-<%= Incident.SeverityLevel.ToLower() %>"><%= Incident.SeverityLevel %></span></dd>
                        <dt class="col-5 text-muted"><%= T("status") %></dt>
                        <dd class="col-7"><span class="badge badge-status-<%= Incident.Status.ToLower() %>"><%= Incident.Status %></span></dd>
                    </dl>
                </div>
            </div>
        </div>
    </div>

    <div class="card sp-card mb-3">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span><i class="bi bi-paperclip text-secondary"></i> <%= T("attachments") %> (<%= Attachments.Count %>)</span>
            <button type="button" class="btn btn-sm btn-outline-secondary"
                    data-bs-toggle="collapse" data-bs-target="#uploadPanel">
                <i class="bi bi-upload me-1"></i><%= T("upload_btn") %>
            </button>
        </div>

        <%-- Upload panel (collapsed by default, stays open on error) --%>
        <div id="uploadPanel" class="collapse<%= !string.IsNullOrEmpty(UploadError) ? " show" : "" %>">
            <div class="card-body border-bottom">
                <% if (!string.IsNullOrEmpty(UploadError)) { %>
                <div class="alert alert-warning py-2 mb-2">
                    <i class="bi bi-exclamation-triangle me-1"></i><%= System.Web.HttpUtility.HtmlEncode(UploadError) %>
                </div>
                <% } %>
                <div class="d-flex gap-2 align-items-end flex-wrap">
                    <div class="flex-grow-1">
                        <label class="form-label mb-1 small">
                            <%= T("attach_files_lbl") %>
                            <span class="text-muted">(<%= T("attach_hint") %>)</span>
                        </label>
                        <asp:FileUpload ID="fuDetailsAttachments" runat="server"
                            AllowMultiple="true" CssClass="form-control form-control-sm"
                            accept=".jpg,.jpeg,.png,.gif,.webp,.pdf,.doc,.docx,.xls,.xlsx" />
                    </div>
                    <asp:Button ID="btnUpload" runat="server"
                        CssClass="btn btn-success btn-sm"
                        OnClick="btnUpload_Click" />
                </div>
                <div class="form-text"><%= T("attach_allowed") %></div>
            </div>
        </div>

        <% if (Attachments.Count == 0) { %>
        <div class="card-body text-center text-muted py-3"><%= T("no_attachments") %></div>
        <% } else { %>
        <div class="card-body p-0">
            <table class="table sp-table mb-0">
                <thead>
                    <tr>
                        <th><%= T("attach_file_col") %></th>
                        <th><%= T("attach_type_col") %></th>
                        <th><%= T("attach_size_col") %></th>
                        <th><%= T("attach_uploaded_col") %></th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var att in Attachments) {
                           var sizeKb    = att.FileSizeBytes / 1024.0;
                           var sizeLabel = sizeKb >= 1024
                               ? string.Format("{0:0.#} MB", sizeKb / 1024.0)
                               : string.Format("{0:0} KB", sizeKb);
                           var icon = att.FileCategory == "image" ? "bi-image" : "bi-file-earmark-text";
                           var downloadUrl = ResolveUrl(
                               "~/Handlers/DownloadAttachment.ashx?incidentId=" + Incident.Id +
                               "&attachmentId=" + att.Id);
                    %>
                    <tr>
                        <td>
                            <i class="bi <%= icon %> me-1 text-muted"></i>
                            <%= System.Web.HttpUtility.HtmlEncode(att.OriginalFileName) %>
                        </td>
                        <td><small class="text-muted"><%= System.Web.HttpUtility.HtmlEncode(att.ContentType) %></small></td>
                        <td><small><%= sizeLabel %></small></td>
                        <td><small><%= att.UploadedAt.ToLocalTime().ToString("dd MMM yyyy HH:mm") %></small></td>
                        <td class="text-end">
                            <a href="<%= downloadUrl %>"
                               class="btn btn-outline-primary btn-sm py-0 px-2 me-1"
                               title="Download">
                                <i class="bi bi-download"></i>
                            </a>
                            <% if (IsManagerOrAdmin) { %>
                            <a href="Details.aspx?id=<%= Incident.Id %>&deleteAttachment=<%= att.Id %>"
                               class="btn btn-outline-danger btn-sm py-0 px-2"
                               title="Delete"
                               onclick="return confirm('<%= T("attach_delete_confirm") %>')">
                                <i class="bi bi-trash"></i>
                            </a>
                            <% } %>
                        </td>
                    </tr>
                    <% } %>
                </tbody>
            </table>
        </div>
        <% } %>
    </div>

    <div class="card sp-card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span><i class="bi bi-check2-square text-primary"></i> <%= T("ca_title") %> (<%= Incident.CorrectiveActions.Count %>)</span>
            <% if (IsSupervisorOrAbove) { %>
            <button type="button" class="btn btn-sm btn-success" data-bs-toggle="modal" data-bs-target="#modalAddAction">
                <i class="bi bi-plus-lg me-1"></i><%= T("add_action") %>
            </button>
            <% } %>
        </div>
        <div class="card-body p-0">
            <table class="table sp-table mb-0">
                <thead>
                    <tr>
                        <th><%= T("action_col") %></th>
                        <th><%= T("assigned_to") %></th>
                        <th><%= T("due_date") %></th>
                        <th><%= T("priority") %></th>
                        <th><%= T("status") %></th>
                        <% if (IsSupervisorOrAbove) { %><th></th><% } %>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var ca in Incident.CorrectiveActions) { %>
                    <tr>
                        <td><%= System.Web.HttpUtility.HtmlEncode(ca.ActionTitle) %></td>
                        <td><%= System.Web.HttpUtility.HtmlEncode(ca.AssignedToFullName) %></td>
                        <td><%= ca.DueDate %></td>
                        <td><span class="badge badge-priority-<%= ca.PriorityLevel.ToLower() %>"><%= ca.PriorityLevel %></span></td>
                        <td><span class="badge badge-status-<%= ca.Status.ToLower() %>"><%= ca.Status %></span></td>
                        <% if (IsSupervisorOrAbove) { %>
                        <td>
                            <% if (ca.Status != ActionStatus.Completed.ToString()) { %>
                            <a href="Details.aspx?id=<%= Incident.Id %>&completeAction=<%= ca.Id %>"
                               class="btn btn-outline-success btn-sm py-0 px-2" title="<%= T("mark_complete") %>"
                               onclick="return confirm('<%= T("confirm_complete") %>')">
                                <i class="bi bi-check-lg"></i>
                            </a>
                            <% } %>
                        </td>
                        <% } %>
                    </tr>
                    <% } %>
                    <% if (Incident.CorrectiveActions.Count == 0) { %>
                    <tr><td colspan="6" class="text-center text-muted py-3"><%= T("no_actions") %></td></tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>

    <% if (IsSupervisorOrAbove) { %>
    <div class="modal fade" id="modalAddAction" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                        <h5 class="modal-title"><i class="bi bi-plus-circle me-2"></i><%= T("new_action") %></h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <% if (!string.IsNullOrEmpty(ActionError)) { %>
                        <div class="alert alert-danger py-2 small"><%= System.Web.HttpUtility.HtmlEncode(ActionError) %></div>
                        <% } %>
                        <div class="mb-3">
                            <label class="form-label"><%= T("action_title_lbl") %> <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtActionTitle" runat="server" CssClass="form-control" MaxLength="200" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label"><%= T("description") %></label>
                            <asp:TextBox ID="txtActionDesc" runat="server" CssClass="form-control"
                                TextMode="MultiLine" Rows="3" />
                        </div>
                        <div class="row g-2">
                            <div class="col-md-6 mb-2">
                                <label class="form-label"><%= T("assigned_to") %> <span class="text-danger">*</span></label>
                                <asp:DropDownList ID="ddlActionUser" runat="server" CssClass="form-select form-select-sm" />
                            </div>
                            <div class="col-md-6 mb-2">
                                <label class="form-label"><%= T("due_date") %> <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtActionDue" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
                            </div>
                            <div class="col-12">
                                <label class="form-label"><%= T("priority") %></label>
                                <asp:DropDownList ID="ddlActionPriority" runat="server" CssClass="form-select form-select-sm">
                                    <asp:ListItem Value="Low">Low</asp:ListItem>
                                    <asp:ListItem Value="Medium" Selected="True">Medium</asp:ListItem>
                                    <asp:ListItem Value="High">High</asp:ListItem>
                                    <asp:ListItem Value="Critical">Critical</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-dismiss="modal"><%= T("cancel") %></button>
                        <asp:Button ID="btnAddAction" runat="server"
                            CssClass="btn btn-success btn-sm" OnClick="btnAddAction_Click" />
                    </div>
            </div>
        </div>
    </div>
    <% } %>

<% } %>
</asp:Content>
