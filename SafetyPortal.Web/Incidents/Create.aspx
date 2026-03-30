<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Create.aspx.cs"
         Inherits="SafetyPortal.Web.Incidents.IncidentCreate" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= T("new_incident_title") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-plus-circle me-2 text-success"></i><%= T("new_incident_title") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<div class="row justify-content-center">
<div class="col-lg-8">

    <% if (!string.IsNullOrEmpty(ErrorMessage)) { %>
    <div class="alert alert-danger"><i class="bi bi-exclamation-circle me-1"></i><%= ErrorMessage %></div>
    <% } %>

    <div class="card sp-card">
        <div class="card-header"><i class="bi bi-clipboard-plus text-success"></i> <%= T("incident_details_hdr") %></div>
        <div class="card-body">
            <form method="post" runat="server">

                <div class="mb-3">
                    <label class="form-label"><%= T("incident_title") %> <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="200" />
                </div>

                <div class="mb-3">
                    <label class="form-label"><%= T("description") %> <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control"
                        TextMode="MultiLine" Rows="4" />
                </div>

                <div class="row g-3">
                    <div class="col-md-6 mb-3">
                        <label class="form-label"><%= T("category") %> <span class="text-danger">*</span></label>
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label"><%= T("department") %> <span class="text-danger">*</span></label>
                        <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-select" />
                    </div>
                </div>

                <div class="row g-3">
                    <div class="col-md-6 mb-3">
                        <label class="form-label"><%= T("incident_date") %> <span class="text-danger">*</span></label>
                        <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label"><%= T("severity") %> <span class="text-danger">*</span></label>
                        <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Low">Low</asp:ListItem>
                            <asp:ListItem Value="Medium">Medium</asp:ListItem>
                            <asp:ListItem Value="High">High</asp:ListItem>
                            <asp:ListItem Value="Critical">Critical</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="mb-3">
                    <label class="form-label"><%= T("location") %></label>
                    <asp:TextBox ID="txtLocation" runat="server" CssClass="form-control" MaxLength="200" />
                </div>

                <div class="mb-4">
                    <label class="form-label"><%= T("assigned_to") %></label>
                    <asp:DropDownList ID="ddlAssign" runat="server" CssClass="form-select" />
                </div>

                <hr class="my-3"/>

                <div class="mb-3">
                    <label class="form-label">
                        <i class="bi bi-paperclip me-1"></i>Attachments
                        <small class="text-muted ms-1">(images up to 5 MB, documents up to 20 MB)</small>
                    </label>
                    <asp:FileUpload ID="fuAttachments" runat="server" AllowMultiple="true"
                        CssClass="form-control"
                        accept=".jpg,.jpeg,.png,.gif,.webp,.pdf,.doc,.docx,.xls,.xlsx" />
                    <div class="form-text">
                        Allowed: JPEG, PNG, GIF, WebP, PDF, DOC, DOCX, XLS, XLSX
                    </div>
                </div>

                <% if (!string.IsNullOrEmpty(UploadErrors)) { %>
                <div class="alert alert-warning py-2">
                    <i class="bi bi-exclamation-triangle me-1"></i>
                    <strong>Some files were not uploaded:</strong><br/>
                    <%= UploadErrors %>
                </div>
                <% } %>

                <div class="d-flex gap-2">
                    <asp:Button ID="btnSave" runat="server"
                        CssClass="btn btn-success" OnClick="btnSave_Click" />
                    <a href="<%= ResolveUrl("~/Incidents/List.aspx") %>"
                       class="btn btn-outline-secondary"><%= T("cancel") %></a>
                </div>

            </form>
        </div>
    </div>
</div>
</div>
</asp:Content>
