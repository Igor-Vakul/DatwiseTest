<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Users.aspx.cs"
         Inherits="SafetyPortal.Web.Admin.UsersAdmin" MasterPageFile="~/Site.Master"
         ValidateRequest="false" %>
<%@ Import Namespace="SafetyPortal.Shared" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= Translate("users_title") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-people me-2 text-primary"></i><%= Translate("users_title") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<div>

    <% if (!string.IsNullOrEmpty(Message)) { %>
    <div class="alert alert-<%= MessageType %> fade show py-2 d-flex align-items-center">
        <span class="me-auto"><%= System.Web.HttpUtility.HtmlEncode(Message) %></span>
        <button type="button" class="btn-close btn-sm ms-3" data-bs-dismiss="alert"></button>
    </div>
    <% } %>

    <div class="row g-3">
        <%-- User list --%>
        <div class="col-lg-8">
            <div class="card sp-card">
                <div class="card-header"><i class="bi bi-people text-primary"></i> <%= Translate("all_users") %></div>
                <div class="card-body p-0">
                    <table class="table sp-table mb-0">
                        <thead>
                            <tr>
                                <th>#</th>
                                <th><%= Translate("full_name") %></th>
                                <th><%= Translate("email_label") %></th>
                                <th><%= Translate("role") %></th>
                                <th><%= Translate("status") %></th>
                                <th><%= Translate("actions") %></th>
                            </tr>
                        </thead>
                        <tbody>
                            <% foreach (var u in Users) { %>
                            <tr class="<%= u.IsActive ? "" : "table-secondary" %>">
                                <td><small class="text-muted"><%= u.Id %></small></td>
                                <td><%= System.Web.HttpUtility.HtmlEncode(u.FullName) %></td>
                                <td><small><%= System.Web.HttpUtility.HtmlEncode(u.Email) %></small></td>
                                <td>
                                    <% string roleColor = u.RoleName == RoleName.Admin.ToString() ? "danger" :
                                           u.RoleName == RoleName.SafetyManager.ToString() ? "primary" :
                                           u.RoleName == RoleName.Supervisor.ToString() ? "info" : "secondary"; %>
                                    <span class="badge bg-<%= roleColor %>"><%= u.RoleName %></span>
                                </td>
                                <td>
                                    <% if (u.IsActive) { %>
                                    <span class="badge bg-success"><%= Translate("active") %></span>
                                    <% } else { %>
                                    <span class="badge bg-secondary"><%= Translate("inactive") %></span>
                                    <% } %>
                                </td>
                                <td class="d-flex gap-1">
                                    <button type="button"
                                        class="btn btn-outline-primary btn-sm py-0 px-2"
                                        title="<%= Translate("edit_user") %>"
                                        onclick="openEdit(<%= u.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(u.FullName) %>', '<%= u.RoleName %>')">
                                        <i class="bi bi-pencil"></i>
                                    </button>
                                    <button type="button"
                                        class="btn btn-outline-info btn-sm py-0 px-2"
                                        title="<%= Translate("send_email") %>"
                                        onclick="openEmail(<%= u.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(u.Email) %>', '<%= System.Web.HttpUtility.JavaScriptStringEncode(u.FullName) %>')">
                                        <i class="bi bi-envelope"></i>
                                    </button>
                                    <a href="Users.aspx?toggle=<%= u.Id %>"
                                       class="btn btn-outline-<%= u.IsActive ? "warning" : "success" %> btn-sm py-0 px-2"
                                       onclick="return confirm('<%= Translate("confirm_toggle") %>')"
                                       title="<%= u.IsActive ? Translate("deactivate") : Translate("activate") %>">
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
                <div class="card-header"><i class="bi bi-person-plus text-success"></i> <%= Translate("new_user") %></div>
                <div class="card-body">
                    <div class="mb-2">
                        <label class="form-label"><%= Translate("full_name") %></label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control form-control-sm" MaxLength="100" />
                    </div>
                    <div class="mb-2">
                        <label class="form-label"><%= Translate("email_label") %></label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control form-control-sm"
                            placeholder="user@datwise.local" TextMode="Email" MaxLength="150" />
                        <asp:RegularExpressionValidator ID="valEmail" runat="server"
                            ControlToValidate="txtEmail"
                            ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$"
                            Display="Dynamic" CssClass="text-danger small"
                            ErrorMessage="Invalid email address" />
                    </div>
                    <div class="mb-2">
                        <label class="form-label"><%= Translate("password_label") %></label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control form-control-sm"
                            TextMode="Password" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("role") %></label>
                        <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select form-select-sm" />
                    </div>
                    <asp:Button ID="btnCreate" runat="server"
                        CssClass="btn btn-success btn-sm w-100" OnClick="btnCreate_Click" />
                </div>
            </div>
        </div>
    </div>

    <%-- Send Email Modal --%>
    <asp:HiddenField ID="hdnEmailUserId" runat="server" />
    <asp:HiddenField ID="hdnEmailAddress" runat="server" />

    <div class="modal fade" id="modalSendEmail" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><i class="bi bi-envelope me-2"></i><%= Translate("send_email") %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <% if (!string.IsNullOrEmpty(EmailError)) { %>
                    <div class="alert alert-danger py-2 small"><%= System.Web.HttpUtility.HtmlEncode(EmailError) %></div>
                    <% } %>
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("email_to_lbl") %></label>
                        <input type="text" id="txtEmailToDisplay" class="form-control form-control-sm"
                               readonly disabled />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("email_subject_lbl") %> <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtEmailSubject" runat="server" CssClass="form-control form-control-sm" MaxLength="200" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("email_body_lbl") %> <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtEmailBody" runat="server" CssClass="form-control form-control-sm"
                            TextMode="MultiLine" Rows="5" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-dismiss="modal"><%= Translate("cancel") %></button>
                    <asp:Button ID="btnSendEmail" runat="server"
                        CssClass="btn btn-info btn-sm" OnClick="btnSendEmail_Click" />
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
                    <h5 class="modal-title"><i class="bi bi-pencil me-2"></i><%= Translate("edit_user") %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("full_name") %> <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtEditName" runat="server" CssClass="form-control" MaxLength="100" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("role") %></label>
                        <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-select" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("password_label") %></label>
                        <asp:TextBox ID="txtEditPassword" runat="server" CssClass="form-control"
                            TextMode="Password" />
                        <div class="form-text"><%= Translate("new_password_hint") %></div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-dismiss="modal"><%= Translate("cancel") %></button>
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

    function openEmail(userId, email, name) {
        document.getElementById('<%= hdnEmailUserId.ClientID %>').value = userId;
        document.getElementById('<%= hdnEmailAddress.ClientID %>').value = email;
        document.getElementById('txtEmailToDisplay').value = name + ' <' + email + '>';
        new bootstrap.Modal(document.getElementById('modalSendEmail')).show();
    }

    // Re-open modals after postback if needed
    <% if (!string.IsNullOrEmpty(hdnEditId.Value)) { %>
    window.addEventListener('DOMContentLoaded', function () {
        new bootstrap.Modal(document.getElementById('modalEditUser')).show();
    });
    <% } %>
    <% if (!string.IsNullOrEmpty(hdnEmailUserId.Value) && !string.IsNullOrEmpty(EmailError)) { %>
    window.addEventListener('DOMContentLoaded', function () {
        document.getElementById('txtEmailToDisplay').value = '<%= System.Web.HttpUtility.JavaScriptStringEncode(hdnEmailAddress.Value) %>';
        new bootstrap.Modal(document.getElementById('modalSendEmail')).show();
    });
    <% } %>
</script>
</asp:Content>
