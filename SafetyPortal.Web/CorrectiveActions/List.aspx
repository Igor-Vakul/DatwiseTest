<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="List.aspx.cs"
         Inherits="SafetyPortal.Web.CorrectiveActions.CorrectiveActionList" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= T("ca_title") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-check2-square me-2 text-info"></i><%= T("ca_title") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="card sp-card mb-3">
        <div class="card-body py-3">
            <div class="row g-2 align-items-end">
                <div class="col-md-3">
                    <label class="form-label"><%= T("status") %></label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select form-select-sm">
                        <asp:ListItem Value=""></asp:ListItem>
                        <asp:ListItem Value="Pending">Pending</asp:ListItem>
                        <asp:ListItem Value="InProgress">In Progress</asp:ListItem>
                        <asp:ListItem Value="Completed">Completed</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-md-2 d-flex gap-1 align-items-end">
                    <asp:Button ID="btnFilter" runat="server"
                        CssClass="btn btn-primary btn-sm" OnClick="btnFilter_Click" />
                    <a href="<%= ResolveUrl("~/CorrectiveActions/List.aspx") %>"
                       class="btn btn-outline-secondary btn-sm"><%= T("reset") %></a>
                </div>
            </div>
        </div>
    </div>

    <div class="card sp-card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span>
                <i class="bi bi-table text-primary"></i>
                <strong><%= Actions.Count %></strong> <%= T("actions_found") %>
            </span>
            <a href="<%= ResolveUrl("~/Handlers/ExportExcel.ashx?type=actions&" + ExportQs) %>"
               class="btn btn-outline-success btn-sm" title="<%= T("export_excel") %>">
                <i class="bi bi-file-earmark-excel me-1"></i><%= T("export_excel") %>
            </a>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover sp-table mb-0">
                    <thead>
                        <tr>
                            <th><%= T("incident_col") %></th>
                            <th><%= T("action_col") %></th>
                            <th><%= T("assigned_to") %></th>
                            <th><%= T("due_date") %></th>
                            <th><%= T("priority") %></th>
                            <th><%= T("status") %></th>
                            <% if (IsSupervisorOrAbove) { %><th><%= T("update") %></th><% } %>
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
                            <% if (IsSupervisorOrAbove) { %>
                            <td>
                                <% if (a.Status != "Completed") { %>
                                <a href="List.aspx?complete=<%= a.Id %>&status=<%= ddlStatus.SelectedValue %>"
                                   class="btn btn-outline-success btn-sm py-0 px-2"
                                   onclick="return confirm('<%= T("confirm_complete") %>')" title="<%= T("mark_complete") %>">
                                    <i class="bi bi-check-lg"></i>
                                </a>
                                <% } %>
                            </td>
                            <% } %>
                        </tr>
                        <% } %>
                        <% if (Actions.Count == 0) { %>
                        <tr><td colspan="7" class="text-center text-muted py-4"><%= T("no_actions_found") %></td></tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

</asp:Content>
