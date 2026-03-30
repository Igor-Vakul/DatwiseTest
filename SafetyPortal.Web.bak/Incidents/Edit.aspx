<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs"
         Inherits="SafetyPortal.Web.Incidents.IncidentEdit" MasterPageFile="~/Site.Master" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server">Edit Incident</asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-pencil-square me-2 text-secondary"></i>Edit Incident
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<div class="row justify-content-center">
<div class="col-lg-8">

    <% if (!string.IsNullOrEmpty(ErrorMessage)) { %>
    <div class="alert alert-danger"><%= ErrorMessage %></div>
    <% } %>

    <div class="card sp-card">
        <div class="card-header"><i class="bi bi-pencil text-secondary"></i> Edit Incident Report</div>
        <div class="card-body">
            <form method="post" runat="server">

                <div class="mb-3">
                    <label class="form-label">Title <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="200" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Description <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control"
                        TextMode="MultiLine" Rows="4" />
                </div>

                <div class="row g-3">
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Category</label>
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label">Department</label>
                        <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-select" />
                    </div>
                </div>

                <div class="row g-3">
                    <div class="col-md-4 mb-3">
                        <label class="form-label">Incident Date</label>
                        <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label">Severity</label>
                        <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Low">Low</asp:ListItem>
                            <asp:ListItem Value="Medium">Medium</asp:ListItem>
                            <asp:ListItem Value="High">High</asp:ListItem>
                            <asp:ListItem Value="Critical">Critical</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label">Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Open">Open</asp:ListItem>
                            <asp:ListItem Value="InProgress">In Progress</asp:ListItem>
                            <asp:ListItem Value="Closed">Closed</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="mb-3">
                    <label class="form-label">Location Details</label>
                    <asp:TextBox ID="txtLocation" runat="server" CssClass="form-control" MaxLength="200" />
                </div>

                <div class="mb-4">
                    <label class="form-label">Assign To</label>
                    <asp:DropDownList ID="ddlAssign" runat="server" CssClass="form-select" />
                </div>

                <div class="d-flex gap-2">
                    <asp:Button ID="btnSave" runat="server" Text="Save Changes"
                        CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <a href="<%= ResolveUrl("~/Incidents/Details.aspx?id=" + IncidentId) %>"
                       class="btn btn-outline-secondary">Cancel</a>
                </div>

            </form>
        </div>
    </div>
</div>
</div>
</asp:Content>
