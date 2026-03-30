<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Users.aspx.cs"
         Inherits="SafetyPortal.Web.Admin.UsersAdmin" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= T("users_title") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-people me-2 text-primary"></i><%= T("users_title") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<div>

    <% if (!string.IsNullOrEmpty(Message)) { %>
    <div class="alert alert-<%= MessageType %> alert-dismissible fade show py-2">
        <%= Message %>
        <button type="button" class="btn-close btn-sm" data-bs-dismiss="alert"></button>
    </div>
    <% } %>

    <div class="row g-3">
        <%-- User list --%>
        <div class="col-lg-8">
            <div class="card sp-card">
                <div class="card-header"><i class="bi bi-people text-primary"></i> <%= T("all_users") %></div>
                <div class="card-body p-0">
                    <table class="table sp-table mb-0">
                        <thead>
                            <tr>
                                <th>#</th>
                                <th><%= T("full_name") %></th>
                                <th><%= T("email_label") %></th>
                                <th><%= T("role") %></th>
                                <th><%= T("status") %></th>
                                <th><%= T("actions") %></th>
                            </tr>
                        </thead>
                        <tbody>
                            <% foreach (var u in Users) { %>
                            <tr class="<%= u.IsActive ? "" : "table-secondary" %>">
                                <td><small class="text-muted"><%= u.Id %></small></td>
                                <td><%= System.Web.HttpUtility.HtmlEncode(u.FullName) %></td>
                                <td><small><%= u.Email %></small></td>
                                <td>
                                    <% string roleColor = u.RoleName == "Admin" ? "danger" :
                                           u.RoleName == "SafetyManager" ? "primary" :
                                           u.RoleName == "Supervisor" ? "info" : "secondary"; %>
                                    <span class="badge bg-<%= roleColor %>"><%= u.RoleName %></span>
                                </td>
                                <td>
                                    <% if (u.IsActive) { %>
                                    <span class="badge bg-success"><%= T("active") %></span>
                                    <% } else { %>
                                    <span class="badge bg-secondary"><%= T("inactive") %></span>
                                    <% } %>
                                </td>
                                <td class="d-flex gap-1">
                                    <button type="button"
                                        class="btn btn-outline-primary btn-sm py-0 px-2"
                                        title="<%= T("edit_user") %>"
                                        onclick="openEdit(<%= u.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(u.FullName) %>', '<%= u.RoleName %>')">
                                        <i class="bi bi-pencil"></i>
                                    </button>
                                    <a href="Users.aspx?toggle=<%= u.Id %>"
                                       class="btn btn-outline-<%= u.IsActive ? "warning" : "success" %> btn-sm py-0 px-2"
                                       onclick="return confirm('<%= T("confirm_toggle") %>')"
                                       title="<%= u.IsActive ? T("deactivate") : T("activate") %>">
                                        <i class="bi bi-toggle-<%= u.IsActive ? "on" : "off" %>"></i>
                                    </a>
                                </td>
                            </tr>
                            <% } %>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <%-- Create user form --%>
        <div class="col-lg-4">
            <div class="card sp-card">
                <div class="card-header"><i class="bi bi-person-plus text-success"></i> <%= T("new_user") %></div>
                <div class="card-body">
                    <div class="mb-2">
                        <label class="form-label"><%= T("full_name") %></label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control form-control-sm" MaxLength="100" />
                    </div>
                    <div class="mb-2">
                        <label class="form-label"><%= T("email_label") %></label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control form-control-sm"
                            placeholder="user@datwise.local" TextMode="Email" MaxLength="150" />
                    </div>
                    <div class="mb-2">
                        <label class="form-label"><%= T("password_label") %></label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control form-control-sm"
                            TextMode="Password" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= T("role") %></label>
                        <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select form-select-sm" />
                    </div>
                    <asp:Button ID="btnCreate" runat="server"
                        CssClass="btn btn-success btn-sm w-100" OnClick="btnCreate_Click" />
                </div>
            </div>
        </div>
    </div>

    <%-- Edit User Modal (inside the same form) --%>
    <asp:HiddenField ID="hdnEditId" runat="server" />

    <div class="modal fade" id="modalEditUser" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-pencil me-2"></i><%= T("edit_user") %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label"><%= T("full_name") %> <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtEditName" runat="server" CssClass="form-control" MaxLength="100" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= T("role") %></label>
                        <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-select" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= T("password_label") %></label>
                        <asp:TextBox ID="txtEditPassword" runat="server" CssClass="form-control"
                            TextMode="Password" />
                        <div class="form-text"><%= T("new_password_hint") %></div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-dismiss="modal"><%= T("cancel") %></button>
                    <asp:Button ID="btnSaveEdit" runat="server"
                        CssClass="btn btn-primary btn-sm" OnClick="btnSaveEdit_Click" />
                </div>
            </div>
        </div>
    </div>

</div>
</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptsContent" runat="server">
<script>
    function openEdit(id, name, roleName) {
        document.getElementById('<%= hdnEditId.ClientID %>').value = id;
        document.getElementById('<%= txtEditName.ClientID %>').value = name;

        var ddl = document.getElementById('<%= ddlEditRole.ClientID %>');
        for (var i = 0; i < ddl.options.length; i++) {
            ddl.options[i].selected = (ddl.options[i].text === roleName);
        }

        new bootstrap.Modal(document.getElementById('modalEditUser')).show();
    }

    // Re-open modal after postback if there was a validation error
    <% if (!string.IsNullOrEmpty(hdnEditId.Value)) { %>
    window.addEventListener('DOMContentLoaded', function () {
        new bootstrap.Modal(document.getElementById('modalEditUser')).show();
    });
    <% } %>
</script>
</asp:Content>
