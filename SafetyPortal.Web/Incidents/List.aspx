<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="List.aspx.cs"
         Inherits="SafetyPortal.Web.Incidents.IncidentList" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= T("incidents_title") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-exclamation-triangle me-2 text-warning"></i><%= T("incidents_title") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <%-- Archive / Active tabs --%>
    <ul class="nav nav-tabs mb-3">
        <li class="nav-item">
            <a class="nav-link <%= !ShowArchived ? "active" : "" %>"
               href="<%= ResolveUrl("~/Incidents/List.aspx") %>">
                <i class="bi bi-list-ul me-1"></i><%= T("active_incidents") %>
            </a>
        </li>
        <% if (IsManagerOrAdmin) { %>
        <li class="nav-item">
            <a class="nav-link <%= ShowArchived ? "active" : "" %>"
               href="<%= ResolveUrl("~/Incidents/List.aspx?archived=true") %>">
                <i class="bi bi-archive me-1"></i><%= T("archived_incidents") %>
            </a>
        </li>
        <% } %>
    </ul>

    <div class="card sp-card mb-3">
        <div class="card-body py-3">
            <div class="row g-2 align-items-end">
                <div class="col-md-3">
                    <label class="form-label"><%= T("search") %></label>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-2">
                    <label class="form-label"><%= T("status") %></label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select form-select-sm">
                        <asp:ListItem Value=""></asp:ListItem>
                        <asp:ListItem Value="Open">Open</asp:ListItem>
                        <asp:ListItem Value="InProgress">In Progress</asp:ListItem>
                        <asp:ListItem Value="Closed">Closed</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label"><%= T("severity") %></label>
                    <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-select form-select-sm">
                        <asp:ListItem Value=""></asp:ListItem>
                        <asp:ListItem Value="Low">Low</asp:ListItem>
                        <asp:ListItem Value="Medium">Medium</asp:ListItem>
                        <asp:ListItem Value="High">High</asp:ListItem>
                        <asp:ListItem Value="Critical">Critical</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label"><%= T("department") %></label>
                    <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-select form-select-sm" />
                </div>
                <div class="col-md-2">
                    <label class="form-label"><%= T("category") %></label>
                    <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select form-select-sm" />
                </div>
                <div class="col-md-1 d-flex gap-1">
                    <asp:Button ID="btnSearch" runat="server"
                        CssClass="btn btn-primary btn-sm" OnClick="btnSearch_Click" />
                    <a href="<%= ResolveUrl(ShowArchived ? "~/Incidents/List.aspx?archived=true" : "~/Incidents/List.aspx") %>"
                       class="btn btn-outline-secondary btn-sm" title="<%= T("reset") %>">
                        <i class="bi bi-x"></i>
                    </a>
                </div>
            </div>
        </div>
    </div>

    <div class="card sp-card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span>
                <i class="bi bi-table text-primary"></i>
                <%= T("showing") %> <strong><%= TotalCount %></strong> <%= T("incidents_found") %>
                <% if (ShowArchived) { %>
                <span class="badge bg-secondary ms-1"><%= T("archived_incidents") %></span>
                <% } %>
            </span>
            <div class="d-flex gap-2">
                <a href="<%= ResolveUrl("~/Handlers/ExportExcel.ashx?type=incidents&" + FilterQs) %>"
                   class="btn btn-outline-success btn-sm" title="<%= T("export_excel") %>">
                    <i class="bi bi-file-earmark-excel me-1"></i><%= T("export_excel") %>
                </a>
                <% if (!ShowArchived) { %>
                <a href="<%= ResolveUrl("~/Incidents/Create.aspx") %>" class="btn btn-success btn-sm">
                    <i class="bi bi-plus-lg me-1"></i><%= T("new_incident") %>
                </a>
                <% } %>
            </div>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover sp-table mb-0">
                    <thead>
                        <tr>
                            <th><%= T("report_number") %></th>
                            <th><%= T("incident_title") %></th>
                            <th><%= T("category") %></th>
                            <th><%= T("department") %></th>
                            <th><%= T("severity") %></th>
                            <th><%= T("status") %></th>
                            <th><%= T("date_col") %></th>
                            <th><%= T("actions") %></th>
                            <th class="text-center"><%= T("ca_count") %></th>
                            <th class="text-center"><i class="bi bi-paperclip" title="Attachments"></i></th>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var inc in Incidents) { %>
                        <tr class="<%= inc.IsArchived ? "table-secondary" : "" %>">
                            <td><a href="<%= ResolveUrl("~/Incidents/Details.aspx?id=" + inc.Id) %>">
                                <code><%= inc.ReportNumber %></code></a></td>
                            <td class="text-truncate" style="max-width:200px"
                                title="<%= System.Web.HttpUtility.HtmlEncode(inc.Title) %>">
                                <%= System.Web.HttpUtility.HtmlEncode(inc.Title) %>
                            </td>
                            <td><span class="badge bg-secondary-subtle text-secondary"><%= System.Web.HttpUtility.HtmlEncode(inc.CategoryName) %></span></td>
                            <td><small class="text-muted"><%= System.Web.HttpUtility.HtmlEncode(inc.DepartmentName) %></small></td>
                            <td><span class="badge badge-severity-<%= inc.SeverityLevel.ToLower() %>"><%= inc.SeverityLevel %></span></td>
                            <td><span class="badge badge-status-<%= inc.Status.ToLower() %>"><%= inc.Status %></span></td>
                            <td><small><%= inc.IncidentDate.ToString("dd MMM yy") %></small></td>
                            <td>
                                <a href="<%= ResolveUrl("~/Incidents/Details.aspx?id=" + inc.Id) %>"
                                   class="btn btn-outline-primary btn-sm py-0 px-2" title="View">
                                    <i class="bi bi-eye"></i>
                                </a>
                                <% if (!inc.IsArchived) { %>
                                <a href="<%= ResolveUrl("~/Incidents/Edit.aspx?id=" + inc.Id) %>"
                                   class="btn btn-outline-secondary btn-sm py-0 px-2" title="<%= T("edit") %>">
                                    <i class="bi bi-pencil"></i>
                                </a>
                                <% } %>
                                <% if (IsManagerOrAdmin && (inc.IsArchived || inc.Status == IncidentStatus.Closed.ToString())) { %>
                                <a href="<%= ResolveUrl("~/Incidents/List.aspx?toggle_archive=" + inc.Id + (ShowArchived ? "&archived=true" : "")) %>"
                                   class="btn btn-outline-<%= inc.IsArchived ? "success" : "warning" %> btn-sm py-0 px-2"
                                   title="<%= inc.IsArchived ? T("unarchive") : T("archive") %>"
                                   onclick="return confirm('<%= inc.IsArchived ? T("confirm_unarchive") : T("confirm_archive") %>');">
                                    <i class="bi bi-<%= inc.IsArchived ? "arrow-counterclockwise" : "archive" %>"></i>
                                </a>
                                <% } %>
                            </td>
                            <td class="text-center">
                                <% if (inc.CorrectiveActionsCount > 0) { %>
                                <span class="badge bg-info-subtle text-info"><%= inc.CorrectiveActionsCount %></span>
                                <% } else { %>
                                <span class="text-muted">—</span>
                                <% } %>
                            </td>
                            <td class="text-center">
                                <% if (inc.AttachmentsCount > 0) { %>
                                <span class="badge bg-secondary-subtle text-secondary">
                                    <i class="bi bi-paperclip"></i> <%= inc.AttachmentsCount %>
                                </span>
                                <% } else { %>
                                <span class="text-muted">—</span>
                                <% } %>
                            </td>
                        </tr>
                        <% } %>
                        <% if (Incidents.Count == 0) { %>
                        <tr><td colspan="10" class="text-center text-muted py-4"><%= T("no_incidents") %></td></tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
        <% if (TotalPages > 1) { %>
        <div class="card-footer d-flex justify-content-between align-items-center">
            <small class="text-muted"><%= T("page") %> <%= CurrentPage %> <%= T("of") %> <%= TotalPages %></small>
            <nav>
                <ul class="pagination pagination-sm mb-0">
                    <li class="page-item <%= CurrentPage == 1 ? "disabled" : "" %>">
                        <a class="page-link" href="?page=<%= CurrentPage - 1 %>&<%= FilterQs %>">‹</a>
                    </li>
                    <% for (int p = Math.Max(1, CurrentPage - 2); p <= Math.Min(TotalPages, CurrentPage + 2); p++) { %>
                    <li class="page-item <%= p == CurrentPage ? "active" : "" %>">
                        <a class="page-link" href="?page=<%= p %>&<%= FilterQs %>"><%= p %></a>
                    </li>
                    <% } %>
                    <li class="page-item <%= CurrentPage == TotalPages ? "disabled" : "" %>">
                        <a class="page-link" href="?page=<%= CurrentPage + 1 %>&<%= FilterQs %>">›</a>
                    </li>
                </ul>
            </nav>
        </div>
        <% } %>
    </div>

</asp:Content>
