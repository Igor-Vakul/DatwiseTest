<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Details.aspx.cs"
         Inherits="SafetyPortal.Web.Incidents.IncidentDetails" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">Incident Details</asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-file-text me-2 text-primary"></i>Incident Details
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

<% if (Incident == null) { %>
    <div class="alert alert-warning">Incident not found.</div>
<% } else { %>

    <%-- Breadcrumb --%>
    <nav aria-label="breadcrumb" class="mb-3">
        <ol class="breadcrumb">
            <li class="breadcrumb-item"><a href="<%= ResolveUrl("~/Incidents/List.aspx") %>">Incidents</a></li>
            <li class="breadcrumb-item active"><code><%= Incident.ReportNumber %></code></li>
        </ol>
    </nav>

    <div class="row g-3 mb-3">
        <%-- Main info card --%>
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
                                <div><small class="text-muted d-block">Category</small><strong><%= Incident.CategoryName %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-building text-muted"></i>
                                <div><small class="text-muted d-block">Department</small><strong><%= Incident.DepartmentName %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-geo-alt text-muted"></i>
                                <div><small class="text-muted d-block">Location</small><strong><%= Incident.LocationDetails ?? "—" %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-calendar-event text-muted"></i>
                                <div><small class="text-muted d-block">Incident Date</small><strong><%= Incident.IncidentDate.ToString("dd MMMM yyyy") %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-person text-muted"></i>
                                <div><small class="text-muted d-block">Reported By</small><strong><%= Incident.ReportedByFullName %></strong></div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="d-flex align-items-center gap-2 mb-2">
                                <i class="bi bi-person-check text-muted"></i>
                                <div><small class="text-muted d-block">Assigned To</small><strong><%= Incident.AssignedToFullName ?? "Unassigned" %></strong></div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="card-footer d-flex gap-2">
                    <a href="<%= ResolveUrl("~/Incidents/Edit.aspx?id=" + Incident.Id) %>"
                       class="btn btn-outline-primary btn-sm">
                        <i class="bi bi-pencil me-1"></i>Edit
                    </a>
                    <a href="<%= ResolveUrl("~/Incidents/List.aspx") %>"
                       class="btn btn-outline-secondary btn-sm">
                        <i class="bi bi-arrow-left me-1"></i>Back to List
                    </a>
                </div>
            </div>
        </div>

        <%-- Report meta card --%>
        <div class="col-lg-4">
            <div class="card sp-card">
                <div class="card-header"><i class="bi bi-info-square text-secondary"></i> Report Info</div>
                <div class="card-body">
                    <dl class="row mb-0 small">
                        <dt class="col-5 text-muted">Report #</dt>
                        <dd class="col-7"><code><%= Incident.ReportNumber %></code></dd>
                        <dt class="col-5 text-muted">Reported At</dt>
                        <dd class="col-7"><%= Incident.ReportedAt.ToString("dd MMM yyyy HH:mm") %></dd>
                        <dt class="col-5 text-muted">Severity</dt>
                        <dd class="col-7"><span class="badge badge-severity-<%= Incident.SeverityLevel.ToLower() %>"><%= Incident.SeverityLevel %></span></dd>
                        <dt class="col-5 text-muted">Status</dt>
                        <dd class="col-7"><span class="badge badge-status-<%= Incident.Status.ToLower() %>"><%= Incident.Status %></span></dd>
                    </dl>
                </div>
            </div>
        </div>
    </div>

    <%-- Corrective Actions --%>
    <div class="card sp-card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span><i class="bi bi-check2-square text-primary"></i> Corrective Actions (<%= Incident.CorrectiveActions.Count %>)</span>
            <% if (IsManagerOrAdmin) { %>
            <button type="button" class="btn btn-sm btn-success" data-bs-toggle="modal" data-bs-target="#modalAddAction">
                <i class="bi bi-plus-lg me-1"></i>Add Action
            </button>
            <% } %>
        </div>
        <div class="card-body p-0">
            <table class="table sp-table mb-0">
                <thead>
                    <tr>
                        <th>Action</th>
                        <th>Assigned To</th>
                        <th>Due Date</th>
                        <th>Priority</th>
                        <th>Status</th>
                        <% if (IsManagerOrAdmin) { %><th></th><% } %>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var ca in Incident.CorrectiveActions) { %>
                    <tr>
                        <td><%= System.Web.HttpUtility.HtmlEncode(ca.ActionTitle) %></td>
                        <td><%= ca.AssignedToFullName %></td>
                        <td><%= ca.DueDate %></td>
                        <td><span class="badge badge-priority-<%= ca.PriorityLevel.ToLower() %>"><%= ca.PriorityLevel %></span></td>
                        <td><span class="badge badge-status-<%= ca.Status.ToLower() %>"><%= ca.Status %></span></td>
                        <% if (IsManagerOrAdmin) { %>
                        <td>
                            <% if (ca.Status != "Completed") { %>
                            <a href="Details.aspx?id=<%= Incident.Id %>&completeAction=<%= ca.Id %>"
                               class="btn btn-outline-success btn-sm py-0 px-2" title="Mark complete"
                               onclick="return confirm('Mark this action as Completed?')">
                                <i class="bi bi-check-lg"></i>
                            </a>
                            <% } %>
                        </td>
                        <% } %>
                    </tr>
                    <% } %>
                    <% if (Incident.CorrectiveActions.Count == 0) { %>
                    <tr><td colspan="6" class="text-center text-muted py-3">No corrective actions yet.</td></tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>

    <%-- Modal: Add Corrective Action --%>
    <% if (IsManagerOrAdmin) { %>
    <div class="modal fade" id="modalAddAction" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <form method="post" runat="server">
                    <div class="modal-header">
                        <h5 class="modal-title"><i class="bi bi-plus-circle me-2"></i>New Corrective Action</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <% if (!string.IsNullOrEmpty(ActionError)) { %>
                        <div class="alert alert-danger py-2 small"><%= ActionError %></div>
                        <% } %>
                        <div class="mb-3">
                            <label class="form-label">Action Title <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtActionTitle" runat="server" CssClass="form-control" MaxLength="200" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Description</label>
                            <asp:TextBox ID="txtActionDesc" runat="server" CssClass="form-control"
                                TextMode="MultiLine" Rows="3" />
                        </div>
                        <div class="row g-2">
                            <div class="col-md-6 mb-2">
                                <label class="form-label">Assign To <span class="text-danger">*</span></label>
                                <asp:DropDownList ID="ddlActionUser" runat="server" CssClass="form-select form-select-sm" />
                            </div>
                            <div class="col-md-6 mb-2">
                                <label class="form-label">Due Date <span class="text-danger">*</span></label>
                                <asp:TextBox ID="txtActionDue" runat="server" CssClass="form-control form-control-sm"
                                    TextMode="Date" />
                            </div>
                            <div class="col-12">
                                <label class="form-label">Priority</label>
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
                        <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                        <asp:Button ID="btnAddAction" runat="server" Text="Save Action"
                            CssClass="btn btn-success btn-sm" OnClick="btnAddAction_Click" />
                    </div>
                </form>
            </div>
        </div>
    </div>
    <% } %>

<% } %>
</asp:Content>
