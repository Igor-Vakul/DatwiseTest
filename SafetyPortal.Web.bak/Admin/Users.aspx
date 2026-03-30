<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Users.aspx.cs"
         Inherits="SafetyPortal.Web.Admin.UsersAdmin" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">Users</asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-people me-2 text-primary"></i>User Management
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

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
                <div class="card-header"><i class="bi bi-people text-primary"></i> All Users</div>
                <div class="card-body p-0">
                    <table class="table sp-table mb-0">
                        <thead>
                            <tr>
                                <th>#</th>
                                <th>Full Name</th>
                                <th>Email</th>
                                <th>Role</th>
                                <th>Status</th>
                                <th>Actions</th>
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
                                    <span class="badge bg-success">Active</span>
                                    <% } else { %>
                                    <span class="badge bg-secondary">Inactive</span>
                                    <% } %>
                                </td>
                                <td>
                                    <a href="Users.aspx?toggle=<%= u.Id %>"
                                       class="btn btn-outline-<%= u.IsActive ? "warning" : "success" %> btn-sm py-0 px-2"
                                       onclick="return confirm('<%= u.IsActive ? "Deactivate" : "Activate" %> this user?')"
                                       title="<%= u.IsActive ? "Deactivate" : "Activate" %>">
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
                <div class="card-header"><i class="bi bi-person-plus text-success"></i> New User</div>
                <div class="card-body">
                    <form method="post" runat="server">
                        <div class="mb-2">
                            <label class="form-label">Full Name</label>
                            <asp:TextBox ID="txtName" runat="server" CssClass="form-control form-control-sm"
                                placeholder="First Last" MaxLength="100" />
                        </div>
                        <div class="mb-2">
                            <label class="form-label">Email</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control form-control-sm"
                                placeholder="user@datwise.local" TextMode="Email" MaxLength="150" />
                        </div>
                        <div class="mb-2">
                            <label class="form-label">Password</label>
                            <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control form-control-sm"
                                TextMode="Password" placeholder="Min 8 chars" />
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Role</label>
                            <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select form-select-sm" />
                        </div>
                        <asp:Button ID="btnCreate" runat="server" Text="Create User"
                            CssClass="btn btn-success btn-sm w-100" OnClick="btnCreate_Click" />
                    </form>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
