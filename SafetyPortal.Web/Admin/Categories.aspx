<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Categories.aspx.cs"
         Inherits="SafetyPortal.Web.Admin.CategoriesAdmin" MasterPageFile="~/Site.Master"
         ValidateRequest="false" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= Translate("categories_title") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle" runat="server">
    <i class="bi bi-tags me-2 text-primary"></i><%= Translate("categories_title") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <% if (!string.IsNullOrEmpty(Message)) { %>
    <div class="alert alert-<%= MessageType %> fade show py-2 d-flex align-items-center">
        <span class="me-auto"><%= System.Web.HttpUtility.HtmlEncode(Message) %></span>
        <button type="button" class="btn-close btn-sm ms-3" data-bs-dismiss="alert"></button>
    </div>
    <% } %>

    <div class="card sp-card">
        <div class="card-header d-flex justify-content-between align-items-center">
            <span><i class="bi bi-tags text-primary"></i> <%= Translate("all_categories") %></span>
            <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#catModal"
                    onclick="openCreate()">
                <i class="bi bi-plus-lg me-1"></i><%= Translate("new_category") %>
            </button>
        </div>
        <div class="card-body p-0">
            <table class="table table-hover sp-table mb-0">
                <thead>
                    <tr>
                        <th><%= Translate("cat_name_col") %></th>
                        <th><%= Translate("cat_desc_col") %></th>
                        <th><%= Translate("status") %></th>
                        <th><%= Translate("actions") %></th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var c in Categories) { %>
                    <tr>
                        <td><strong><%= System.Web.HttpUtility.HtmlEncode(c.Name) %></strong></td>
                        <td><small class="text-muted"><%= System.Web.HttpUtility.HtmlEncode(c.Description ?? "—") %></small></td>
                        <td>
                            <a href="?toggle=<%= c.Id %>"
                               class="badge <%= c.IsActive ? "bg-success-subtle text-success" : "bg-secondary-subtle text-secondary" %> text-decoration-none"
                               title="<%= Translate("click_to_toggle") %>">
                                <%= c.IsActive ? Translate("active") : Translate("inactive") %>
                            </a>
                        </td>
                        <td>
                            <button type="button" class="btn btn-outline-secondary btn-sm py-0 px-2"
                                    onclick="openEdit(<%= c.Id %>,'<%= System.Web.HttpUtility.JavaScriptStringEncode(c.Name) %>','<%= System.Web.HttpUtility.JavaScriptStringEncode(c.Description ?? "") %>',<%= c.IsActive ? "true" : "false" %>)"
                                    data-bs-toggle="modal" data-bs-target="#catModal">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-sm py-0 px-2"
                                    onclick="confirmDelete(<%= c.Id %>,'<%= System.Web.HttpUtility.JavaScriptStringEncode(c.Name) %>')">
                                <i class="bi bi-trash"></i>
                            </button>
                        </td>
                    </tr>
                    <% } %>
                    <% if (Categories.Count == 0) { %>
                    <tr><td colspan="4" class="text-center text-muted py-4"><%= Translate("no_categories") %></td></tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>

    <%-- Create / Edit modal --%>
    <div class="modal fade" id="catModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="catModalTitle"><%= Translate("categories_title") %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("cat_name_label") %> <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= Translate("cat_desc_label") %></label>
                        <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control"
                                     TextMode="MultiLine" Rows="3" MaxLength="255"
                                     style="resize:none" />
                    </div>
                    <div class="mb-3" id="activeRow" style="display:none">
                        <div class="form-check form-switch">
                            <input type="checkbox" id="chkActiveUI" class="form-check-input" role="switch" />
                            <asp:HiddenField ID="hfActive" runat="server" Value="true" />
                            <label class="form-check-label" for="chkActiveUI"><%= Translate("active") %></label>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal"><%= Translate("cancel") %></button>
                    <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary btn-sm"
                                Text="Save" OnClick="btnSave_Click" />
                </div>
            </div>
        </div>
    </div>

    <%-- Delete confirmation modal --%>
    <div class="modal fade" id="deleteModal" tabindex="-1">
        <div class="modal-dialog modal-sm">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title"><%= Translate("cat_delete_confirm") %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <p><%= Translate("delete") %> <strong id="deleteName"></strong>?</p>
                    <p class="text-muted small"><%= Translate("cat_delete_hint") %></p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal"><%= Translate("cancel") %></button>
                    <asp:Button ID="btnDelete" runat="server" CssClass="btn btn-danger btn-sm"
                                Text="Delete" OnClick="btnDelete_Click" />
                    <asp:HiddenField ID="hfDeleteId" runat="server" Value="0" />
                </div>
            </div>
        </div>
    </div>

</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptsContent" runat="server">
<script>
    var _newCat  = '<%= Translate("new_category") %>';
    var _editCat = '<%= Translate("categories_title") %>';

    document.getElementById('chkActiveUI').addEventListener('change', function () {
        document.getElementById('<%= hfActive.ClientID %>').value = this.checked;
    });

    function openCreate() {
        document.getElementById('catModalTitle').textContent = _newCat;
        document.getElementById('<%= hfEditId.ClientID %>').value = '0';
        document.getElementById('<%= txtName.ClientID %>').value = '';
        document.getElementById('<%= txtDescription.ClientID %>').value = '';
        document.getElementById('chkActiveUI').checked = true;
        document.getElementById('<%= hfActive.ClientID %>').value = 'true';
        document.getElementById('activeRow').style.display = 'none';
    }

    function openEdit(id, name, description, isActive) {
        document.getElementById('catModalTitle').textContent = _editCat;
        document.getElementById('<%= hfEditId.ClientID %>').value = id;
        document.getElementById('<%= txtName.ClientID %>').value = name;
        document.getElementById('<%= txtDescription.ClientID %>').value = description;
        document.getElementById('chkActiveUI').checked = isActive;
        document.getElementById('<%= hfActive.ClientID %>').value = isActive;
        document.getElementById('activeRow').style.display = '';
    }

    function confirmDelete(id, name) {
        document.getElementById('<%= hfDeleteId.ClientID %>').value = id;
        document.getElementById('deleteName').textContent = name;
        new bootstrap.Modal(document.getElementById('deleteModal')).show();
    }
</script>
</asp:Content>
