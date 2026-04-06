<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="List.aspx.cs"
         Inherits="SafetyPortal.Web.CorrectiveActions.CorrectiveActionList" MasterPageFile="~/Site.Master" %>
<%@ Import Namespace="SafetyPortal.Shared" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= Translate("ca_title") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-check2-square me-2 text-info"></i><%= Translate("ca_title") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <div class="card sp-card mb-3">
        <div class="card-body py-3">
            <div class="row g-2 align-items-end">
                <div class="col-md-3">
                    <label class="form-label"><%= Translate("status") %></label>
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
                       class="btn btn-outline-secondary btn-sm"><%= Translate("reset") %></a>
                </div>
            </div>
        </div>
    </div>

    <div class="card sp-card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span>
                <i class="bi bi-table text-primary"></i>
                <strong><%= Actions.Count %></strong> <%= Translate("actions_found") %>
            </span>
            <a href="<%= ResolveUrl("~/Handlers/ExportExcel.ashx?type=actions&" + ExportQs) %>"
               class="btn btn-outline-success btn-sm" title="<%= Translate("export_excel") %>">
                <i class="bi bi-file-earmark-excel me-1"></i><%= Translate("export_excel") %>
            </a>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover sp-table mb-0">
                    <thead>
                        <tr>
                            <th><%= Translate("incident_col") %></th>
                            <th><%= Translate("action_col") %></th>
                            <th><%= Translate("assigned_to") %></th>
                            <th><%= Translate("due_date") %></th>
                            <th><%= Translate("priority") %></th>
                            <th><%= Translate("status") %></th>
                            <% if (IsSupervisorOrAbove) { %><th><%= Translate("update") %></th><% } %>
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
                            <td><%= System.Web.HttpUtility.HtmlEncode(a.AssignedToFullName) %></td>
                            <td>
                                <% if (IsOverdue(a)) { %>
                                <span class="text-danger fw-semibold"><i class="bi bi-alarm me-1"></i><%= a.DueDate %></span>
                                <% } else { %>
                                <%= a.DueDate %>
                                <% } %>
                            </td>
                            <td><span class="badge badge-priority-<%= a.PriorityLevel.ToLower() %>"><%= a.PriorityLevel %></span></td>
                            <td><span class="badge rounded-pill" style="background-color:<%= System.Web.HttpUtility.HtmlAttributeEncode(ActionStatusColors.ContainsKey(a.Status) ? ActionStatusColors[a.Status] : "#6c757d") %>"><%= a.Status %></span></td>
                            <% if (IsSupervisorOrAbove) { %>
                            <td>
                                <% if (a.Status != ActionStatus.Completed.ToString()) { %>
                                <a href="List.aspx?complete=<%= a.Id %>&status=<%= ddlStatus.SelectedValue %>"
                                   class="btn btn-outline-success btn-sm py-0 px-2"
                                   onclick="return confirm('<%= Translate("confirm_complete") %>')" title="<%= Translate("mark_complete") %>">
                                    <i class="bi bi-check-lg"></i>
                                </a>
                                <% } %>
                            </td>
                            <% } %>
                        </tr>
                        <% } %>
                        <% if (Actions.Count == 0) { %>
                        <tr><td colspan="7" class="text-center text-muted py-4"><%= Translate("no_actions_found") %></td></tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

</asp:Content>
