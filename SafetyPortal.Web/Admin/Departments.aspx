<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Departments.aspx.cs"
         Inherits="SafetyPortal.Web.Admin.DepartmentsAdmin" MasterPageFile="~/Site.Master"
         ValidateRequest="false" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= T("departments_title") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle" runat="server">
    <i class="bi bi-building me-2 text-primary"></i><%= T("departments_title") %>
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
            <span><i class="bi bi-building text-primary"></i> <%= T("all_departments") %></span>
            <button type="button" class="btn btn-success btn-sm" data-bs-toggle="modal" data-bs-target="#deptModal"
                    onclick="openCreate()">
                <i class="bi bi-plus-lg me-1"></i><%= T("new_department") %>
            </button>
        </div>
        <div class="card-body p-0">
            <table class="table table-hover sp-table mb-0">
                <thead>
                    <tr>
                        <th><%= T("dept_name_col") %></th>
                        <th><%= T("dept_location_col") %></th>
                        <th><%= T("dept_color_col") %></th>
                        <th><%= T("status") %></th>
                        <th><%= T("actions") %></th>
                    </tr>
                </thead>
                <tbody>
                    <% foreach (var d in Departments) { %>
                    <tr>
                        <td><strong><%= System.Web.HttpUtility.HtmlEncode(d.Name) %></strong></td>
                        <td><small class="text-muted"><%= System.Web.HttpUtility.HtmlEncode(d.LocationName ?? "—") %></small></td>
                        <td>
                            <span class="d-inline-flex align-items-center gap-2">
                                <span style="display:inline-block;width:20px;height:20px;border-radius:4px;background:<%= d.Color %>"></span>
                                <code><%= d.Color %></code>
                            </span>
                        </td>
                        <td>
                            <a href="?toggle=<%= d.Id %>"
                               class="badge <%= d.IsActive ? "bg-success-subtle text-success" : "bg-secondary-subtle text-secondary" %> text-decoration-none"
                               title="<%= T("click_to_toggle") %>">
                                <%= d.IsActive ? T("active") : T("inactive") %>
                            </a>
                        </td>
                        <td>
                            <button type="button" class="btn btn-outline-secondary btn-sm py-0 px-2"
                                    onclick="openEdit(<%= d.Id %>,'<%= System.Web.HttpUtility.JavaScriptStringEncode(d.Name) %>','<%= System.Web.HttpUtility.JavaScriptStringEncode(d.LocationName ?? "") %>','<%= d.Color %>',<%= d.IsActive ? "true" : "false" %>)"
                                    data-bs-toggle="modal" data-bs-target="#deptModal">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-sm py-0 px-2"
                                    onclick="confirmDelete(<%= d.Id %>,'<%= System.Web.HttpUtility.JavaScriptStringEncode(d.Name) %>')">
                                <i class="bi bi-trash"></i>
                            </button>
                        </td>
                    </tr>
                    <% } %>
                    <% if (Departments.Count == 0) { %>
                    <tr><td colspan="5" class="text-center text-muted py-4"><%= T("no_departments") %></td></tr>
                    <% } %>
                </tbody>
            </table>
        </div>
    </div>

    <%-- Create / Edit modal --%>
    <div class="modal fade" id="deptModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title" id="deptModalTitle"><%= T("departments_title") %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />
                    <div class="mb-3">
                        <label class="form-label"><%= T("dept_name_label") %> <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="100" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= T("dept_location_label") %></label>
                        <asp:TextBox ID="txtLocation" runat="server" CssClass="form-control" MaxLength="100" />
                        <datalist id="locationsList">
                            <% foreach (var loc in new System.Collections.Generic.HashSet<string>(
                                   Departments.Where(d => !string.IsNullOrEmpty(d.LocationName))
                                              .Select(d => d.LocationName)).OrderBy(l => l)) { %>
                            <option value="<%= System.Web.HttpUtility.HtmlEncode(loc) %>"></option>
                            <% } %>
                        </datalist>
                    </div>
                    <div class="mb-3">
                        <label class="form-label"><%= T("dept_color_label") %></label>
                        <div class="d-flex gap-2 align-items-center">
                            <input type="color" id="colorPickerInput" class="form-control form-control-color"
                                   style="width:60px;padding:2px 4px" value="#6c757d"
                                   oninput="document.getElementById('<%= hfColor.ClientID %>').value = this.value" />
                            <asp:HiddenField ID="hfColor" runat="server" Value="#6c757d" />
                            <small class="text-muted"><%= T("dept_color_hint") %></small>
                        </div>
                    </div>
                    <div class="mb-3" id="activeRow" style="display:none">
                        <div class="form-check form-switch">
                            <input type="checkbox" id="chkActiveUI" class="form-check-input" role="switch" />
                            <asp:HiddenField ID="hfActive" runat="server" Value="true" />
                            <label class="form-check-label" for="chkActiveUI"><%= T("active") %></label>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal"><%= T("cancel") %></button>
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
                    <h5 class="modal-title"><%= T("dept_delete_confirm") %></h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <p><%= T("delete") %> <strong id="deleteName"></strong>?</p>
                    <p class="text-muted small"><%= T("dept_delete_hint") %></p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal"><%= T("cancel") %></button>
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
    var _newDept  = '<%= T("new_department").Replace("+","").Trim() %>';
    var _editDept = '<%= T("departments_title") %>';

    document.getElementById('chkActiveUI').addEventListener('change', function () {
        document.getElementById('<%= hfActive.ClientID %>').value = this.checked;
    });

    function openCreate() {
        document.getElementById('deptModalTitle').textContent = _newDept;
        document.getElementById('<%= hfEditId.ClientID %>').value = '0';
        document.getElementById('<%= txtName.ClientID %>').value = '';
        document.getElementById('<%= txtLocation.ClientID %>').value = '';
        document.getElementById('colorPickerInput').value = '#6c757d';
        document.getElementById('<%= hfColor.ClientID %>').value = '#6c757d';
        document.getElementById('chkActiveUI').checked = true;
        document.getElementById('<%= hfActive.ClientID %>').value = 'true';
        document.getElementById('activeRow').style.display = 'none';
    }

    function openEdit(id, name, location, color, isActive) {
        document.getElementById('deptModalTitle').textContent = _editDept;
        document.getElementById('<%= hfEditId.ClientID %>').value = id;
        document.getElementById('<%= txtName.ClientID %>').value = name;
        document.getElementById('<%= txtLocation.ClientID %>').value = location;
        document.getElementById('colorPickerInput').value = color;
        document.getElementById('<%= hfColor.ClientID %>').value = color;
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
