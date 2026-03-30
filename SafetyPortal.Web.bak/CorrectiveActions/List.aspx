<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="List.aspx.cs"
         Inherits="SafetyPortal.Web.CorrectiveActions.CorrectiveActionList" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">Corrective Actions</asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-check2-square me-2 text-info"></i>Corrective Actions
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <%-- Filter bar --%>
    <div class="card sp-card mb-3">
        <div class="card-body py-3">
            <form method="get" runat="server" class="row g-2 align-items-end">
                <div class="col-md-3">
                    <label class="form-label">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select form-select-sm">
                        <asp:ListItem Value="">All Statuses</asp:ListItem>
                        <asp:ListItem Value="Pending">Pending</asp:ListItem>
                        <asp:ListItem Value="InProgress">In Progress</asp:ListItem>
                        <asp:ListItem Value="Completed">Completed</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <asp:Button ID="btnFilter" runat="server" Text="Filter"
                        CssClass="btn btn-primary btn-sm" OnClick="btnFilter_Click" />
                    <a href="<%= ResolveUrl("~/CorrectiveActions/List.aspx") %>"
                       class="btn btn-outline-secondary btn-sm">Reset</a>
                </div>
            </form>
        </div>
    </div>

    <div class="card sp-card">
        <div class="card-header">
            <i class="bi bi-table text-primary"></i>
            <strong><%= Actions.Count %></strong> action(s) found
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover sp-table mb-0">
                    <thead>
                        <tr>
                            <th>Incident</th>
                            <th>Action</th>
                            <th>Assigned To</th>
                            <th>Due Date</th>
                            <th>Priority</th>
                            <th>Status</th>
                            <% if (IsManagerOrAdmin) { %><th>Update</th><% } %>
                        </tr>
                    </thead>
                    <tbody>
                        <% foreach (var a in Actions) { %>
                        <tr>
                            <td>
                                <a href="<%= ResolveUrl("~/Incidents/Details.aspx?id=" + a.ReportId) %>">
                                    <code><%= a.ReportNumber %></code>
                                </a>
                            </td>
                            <td class="text-truncate" style="max-width:220px"
                                title="<%= System.Web.HttpUtility.HtmlEncode(a.ActionTitle) %>">
                                <%= System.Web.HttpUtility.HtmlEncode(a.ActionTitle) %>
                            </td>
                            <td><%= a.AssignedToFullName %></td>
                            <td>
                                <% if (IsOverdue(a)) { %>
                                <span class="text-danger fw-semibold"><i class="bi bi-alarm me-1"></i><%= a.DueDate %></span>
                                <% } else { %>
                                <%= a.DueDate %>
                                <% } %>
                            </td>
                            <td><span class="badge badge-priority-<%= a.PriorityLevel.ToLower() %>"><%= a.PriorityLevel %></span></td>
                            <td><span class="badge badge-status-<%= a.Status.ToLower() %>"><%= a.Status %></span></td>
                            <% if (IsManagerOrAdmin) { %>
                            <td>
                                <% if (a.Status != "Completed") { %>
                                <a href="List.aspx?complete=<%= a.Id %>&status=<%= ddlStatus.SelectedValue %>"
                                   class="btn btn-outline-success btn-sm py-0 px-2"
                                   onclick="return confirm('Mark as Completed?')" title="Complete">
                                    <i class="bi bi-check-lg"></i>
                                </a>
                                <% } %>
                            </td>
                            <% } %>
                        </tr>
                        <% } %>
                        <% if (Actions.Count == 0) { %>
                        <tr><td colspan="7" class="text-center text-muted py-4">No corrective actions found.</td></tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

</asp:Content>
