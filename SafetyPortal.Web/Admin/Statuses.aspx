<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Statuses.aspx.cs"
         Inherits="SafetyPortal.Web.Admin.StatusesAdmin" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">Status Management</asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle" runat="server">
    <i class="bi bi-sliders me-2 text-primary"></i>Status Management
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

<% if (!string.IsNullOrEmpty(Message)) { %>
<div class="alert alert-<%= MessageType %> fade show py-2 d-flex align-items-center">
    <span class="me-auto"><%= System.Web.HttpUtility.HtmlEncode(Message) %></span>
    <button type="button" class="btn-close btn-sm ms-3" data-bs-dismiss="alert"></button>
</div>
<% } %>

<div class="row g-3">

    <%-- ── Incident Statuses ──────────────────────────────────────────────── --%>
    <div class="col-lg-4">
        <div class="card sp-card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <span><i class="bi bi-flag text-primary"></i> Incident Statuses</span>
                <button type="button" class="btn btn-success btn-sm"
                        onclick="openCreate('incident')" data-bs-toggle="modal" data-bs-target="#statusModal">
                    <i class="bi bi-plus-lg"></i>
                </button>
            </div>
            <div class="card-body p-0">
                <table class="table table-hover sp-table mb-0">
                    <thead><tr><th>Name</th><th>Closing</th><th>Active</th><th></th></tr></thead>
                    <tbody>
                        <% foreach (var s in IncidentStatuses) { %>
                        <tr>
                            <td>
                                <span class="badge rounded-pill" style="background-color:<%= System.Web.HttpUtility.HtmlAttributeEncode(s.Color) %>"><%= System.Web.HttpUtility.HtmlEncode(s.Name) %></span>
                                <% if (s.IsSystem) { %><span class="badge bg-secondary ms-1" style="font-size:.6rem">system</span><% } %>
                            </td>
                            <td>
                                <% if (s.IsClosing) { %>
                                <span class="badge bg-success-subtle text-success">Yes</span>
                                <% } else { %>
                                <span class="text-muted small">No</span>
                                <% } %>
                            </td>
                            <td>
                                <span class="badge <%= s.IsActive ? "bg-success" : "bg-secondary" %>">
                                    <%= s.IsActive ? "Active" : "Inactive" %>
                                </span>
                            </td>
                            <td class="text-end">
                                <button type="button" class="btn btn-outline-secondary btn-sm py-0 px-1"
                                        onclick="openEdit('incident', <%= s.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Name) %>', <%= s.IsClosing ? "true" : "false" %>, <%= s.IsActive ? "true" : "false" %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Color) %>')"
                                        data-bs-toggle="modal" data-bs-target="#statusModal">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <% if (!s.IsSystem) { %>
                                <button type="button" class="btn btn-outline-danger btn-sm py-0 px-1"
                                        onclick="confirmDelete('incident', <%= s.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Name) %>')">
                                    <i class="bi bi-trash"></i>
                                </button>
                                <% } %>
                            </td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- ── Severity Levels ────────────────────────────────────────────────── --%>
    <div class="col-lg-4">
        <div class="card sp-card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <span><i class="bi bi-exclamation-triangle text-warning"></i> Severity Levels</span>
                <button type="button" class="btn btn-success btn-sm"
                        onclick="openCreate('severity')" data-bs-toggle="modal" data-bs-target="#statusModal">
                    <i class="bi bi-plus-lg"></i>
                </button>
            </div>
            <div class="card-body p-0">
                <table class="table table-hover sp-table mb-0">
                    <thead><tr><th>Name</th><th>Active</th><th></th></tr></thead>
                    <tbody>
                        <% foreach (var s in SeverityLevels) { %>
                        <tr>
                            <td>
                                <span class="badge rounded-pill" style="background-color:<%= System.Web.HttpUtility.HtmlAttributeEncode(s.Color) %>"><%= System.Web.HttpUtility.HtmlEncode(s.Name) %></span>
                                <% if (s.IsSystem) { %><span class="badge bg-secondary ms-1" style="font-size:.6rem">system</span><% } %>
                            </td>
                            <td>
                                <span class="badge <%= s.IsActive ? "bg-success" : "bg-secondary" %>">
                                    <%= s.IsActive ? "Active" : "Inactive" %>
                                </span>
                            </td>
                            <td class="text-end">
                                <button type="button" class="btn btn-outline-secondary btn-sm py-0 px-1"
                                        onclick="openEdit('severity', <%= s.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Name) %>', false, <%= s.IsActive ? "true" : "false" %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Color) %>')"
                                        data-bs-toggle="modal" data-bs-target="#statusModal">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <% if (!s.IsSystem) { %>
                                <button type="button" class="btn btn-outline-danger btn-sm py-0 px-1"
                                        onclick="confirmDelete('severity', <%= s.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Name) %>')">
                                    <i class="bi bi-trash"></i>
                                </button>
                                <% } %>
                            </td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- ── Action Statuses ────────────────────────────────────────────────── --%>
    <div class="col-lg-4">
        <div class="card sp-card">
            <div class="card-header d-flex justify-content-between align-items-center">
                <span><i class="bi bi-check2-square text-success"></i> Action Statuses</span>
                <button type="button" class="btn btn-success btn-sm"
                        onclick="openCreate('action')" data-bs-toggle="modal" data-bs-target="#statusModal">
                    <i class="bi bi-plus-lg"></i>
                </button>
            </div>
            <div class="card-body p-0">
                <table class="table table-hover sp-table mb-0">
                    <thead><tr><th>Name</th><th>Completed</th><th>Active</th><th></th></tr></thead>
                    <tbody>
                        <% foreach (var s in ActionStatuses) { %>
                        <tr>
                            <td>
                                <span class="badge rounded-pill" style="background-color:<%= System.Web.HttpUtility.HtmlAttributeEncode(s.Color) %>"><%= System.Web.HttpUtility.HtmlEncode(s.Name) %></span>
                                <% if (s.IsSystem) { %><span class="badge bg-secondary ms-1" style="font-size:.6rem">system</span><% } %>
                            </td>
                            <td>
                                <% if (s.IsCompleted) { %>
                                <span class="badge bg-success-subtle text-success">Yes</span>
                                <% } else { %>
                                <span class="text-muted small">No</span>
                                <% } %>
                            </td>
                            <td>
                                <span class="badge <%= s.IsActive ? "bg-success" : "bg-secondary" %>">
                                    <%= s.IsActive ? "Active" : "Inactive" %>
                                </span>
                            </td>
                            <td class="text-end">
                                <button type="button" class="btn btn-outline-secondary btn-sm py-0 px-1"
                                        onclick="openEdit('action', <%= s.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Name) %>', <%= s.IsCompleted ? "true" : "false" %>, <%= s.IsActive ? "true" : "false" %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Color) %>')"
                                        data-bs-toggle="modal" data-bs-target="#statusModal">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <% if (!s.IsSystem) { %>
                                <button type="button" class="btn btn-outline-danger btn-sm py-0 px-1"
                                        onclick="confirmDelete('action', <%= s.Id %>, '<%= System.Web.HttpUtility.JavaScriptStringEncode(s.Name) %>')">
                                    <i class="bi bi-trash"></i>
                                </button>
                                <% } %>
                            </td>
                        </tr>
                        <% } %>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

</div>

<%-- ── Shared Create/Edit Modal ─────────────────────────────────────────── --%>
<asp:HiddenField ID="hfType" runat="server" />
<asp:HiddenField ID="hfId" runat="server" Value="0" />

<div class="modal fade" id="statusModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modalTitle">Status</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label class="form-label">Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="50" />
                </div>
                <div class="mb-3">
                    <label class="form-label">Color</label>
                    <asp:HiddenField ID="hfColor" runat="server" Value="#6c757d" />
                    <div class="d-flex align-items-center gap-2">
                        <input type="color" id="colorPicker" class="form-control form-control-color"
                               value="#6c757d"
                               oninput="document.getElementById('<%= hfColor.ClientID %>').value = this.value; document.getElementById('colorPreview').style.backgroundColor = this.value;" />
                        <span id="colorPreview" class="badge rounded-pill px-3"
                              style="background-color:#6c757d;min-width:80px">Preview</span>
                    </div>
                </div>
                <div class="mb-3" id="rowFlag" style="display:none">
                    <div class="form-check form-switch">
                        <asp:HiddenField ID="hfFlag" runat="server" Value="false" />
                        <input type="checkbox" id="chkFlag" class="form-check-input" role="switch"
                               onchange="document.getElementById('<%= hfFlag.ClientID %>').value = this.checked" />
                        <label class="form-check-label" id="lblFlag" for="chkFlag">Flag</label>
                    </div>
                </div>
                <div class="mb-3" id="rowActive" style="display:none">
                    <div class="form-check form-switch">
                        <asp:HiddenField ID="hfActive" runat="server" Value="true" />
                        <input type="checkbox" id="chkActive" class="form-check-input" role="switch" checked
                               onchange="document.getElementById('<%= hfActive.ClientID %>').value = this.checked" />
                        <label class="form-check-label" for="chkActive">Active</label>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary btn-sm"
                            Text="Save" OnClick="btnSave_Click" />
            </div>
        </div>
    </div>
</div>

<%-- ── Delete Confirmation Modal ───────────────────────────────────────── --%>
<asp:HiddenField ID="hfDeleteId" runat="server" Value="0" />
<asp:HiddenField ID="hfDeleteType" runat="server" />

<div class="modal fade" id="deleteModal" tabindex="-1">
    <div class="modal-dialog modal-sm">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Delete</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <p>Delete <strong id="deleteName"></strong>?</p>
                <p class="text-muted small">Cannot be deleted if used by existing records.</p>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                <asp:Button ID="btnDelete" runat="server" CssClass="btn btn-danger btn-sm"
                            Text="Delete" OnClick="btnDelete_Click" />
            </div>
        </div>
    </div>
</div>

</asp:Content>

<asp:Content ContentPlaceHolderID="ScriptsContent" runat="server">
<script>
    var _currentType = '';

    function setColor(hex) {
        var safe = hex || '#6c757d';
        document.getElementById('<%= hfColor.ClientID %>').value = safe;
        document.getElementById('colorPicker').value = safe;
        document.getElementById('colorPreview').style.backgroundColor = safe;
    }

    function openCreate(type) {
        _currentType = type;
        document.getElementById('<%= hfType.ClientID %>').value = type;
        document.getElementById('<%= hfId.ClientID %>').value = '0';
        document.getElementById('<%= txtName.ClientID %>').value = '';
        document.getElementById('<%= hfFlag.ClientID %>').value = 'false';
        document.getElementById('<%= hfActive.ClientID %>').value = 'true';
        document.getElementById('chkFlag').checked = false;
        document.getElementById('chkActive').checked = true;
        document.getElementById('rowActive').style.display = 'none';
        document.getElementById('modalTitle').textContent = 'New ' + label(type);
        setupFlag(type);
        document.getElementById('rowFlag').style.display = type === 'severity' ? 'none' : '';
        setColor('#6c757d');
    }

    function openEdit(type, id, name, flag, isActive, color) {
        _currentType = type;
        document.getElementById('<%= hfType.ClientID %>').value = type;
        document.getElementById('<%= hfId.ClientID %>').value = id;
        document.getElementById('<%= txtName.ClientID %>').value = name;
        document.getElementById('<%= hfFlag.ClientID %>').value = flag;
        document.getElementById('<%= hfActive.ClientID %>').value = isActive;
        document.getElementById('chkFlag').checked = flag;
        document.getElementById('chkActive').checked = isActive;
        document.getElementById('rowActive').style.display = '';
        document.getElementById('modalTitle').textContent = 'Edit ' + label(type);
        setupFlag(type);
        document.getElementById('rowFlag').style.display = type === 'severity' ? 'none' : '';
        setColor(color);
    }

    function setupFlag(type) {
        var lbl = type === 'incident' ? 'Closing status (allows archiving)' : 'Marks action as completed';
        document.getElementById('lblFlag').textContent = lbl;
    }

    function label(type) {
        return type === 'incident' ? 'Incident Status' : type === 'severity' ? 'Severity Level' : 'Action Status';
    }

    function confirmDelete(type, id, name) {
        document.getElementById('<%= hfDeleteId.ClientID %>').value = id;
        document.getElementById('<%= hfDeleteType.ClientID %>').value = type;
        document.getElementById('deleteName').textContent = name;
        new bootstrap.Modal(document.getElementById('deleteModal')).show();
    }

    <% if (!string.IsNullOrEmpty(hfId.Value) && hfId.Value != "0") { %>
    window.addEventListener('DOMContentLoaded', function () {
        new bootstrap.Modal(document.getElementById('statusModal')).show();
    });
    <% } %>
</script>
</asp:Content>
