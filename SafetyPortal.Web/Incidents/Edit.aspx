<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Edit.aspx.cs"
         Inherits="SafetyPortal.Web.Incidents.IncidentEdit" MasterPageFile="~/Site.Master"
         ValidateRequest="false" %>

<asp:Content ContentPlaceHolderID="TitleContent" runat="server"><%= Translate("edit_incident") %></asp:Content>
<asp:Content ContentPlaceHolderID="PageTitle"    runat="server">
    <i class="bi bi-pencil-square me-2 text-secondary"></i><%= Translate("edit_incident") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<div class="row justify-content-center">
<div class="col-lg-8">

    <% if (!string.IsNullOrEmpty(ErrorMessage)) { %>
    <div class="alert alert-danger"><%= System.Web.HttpUtility.HtmlEncode(ErrorMessage) %></div>
    <% } %>

    <div class="card sp-card">
        <div class="card-header"><i class="bi bi-pencil text-secondary"></i> <%= Translate("edit_incident") %></div>
        <div class="card-body">
            <div>

                <div class="mb-3">
                    <label class="form-label"><%= Translate("incident_title") %> <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="200" />
                </div>

                <div class="mb-3">
                    <label class="form-label"><%= Translate("description") %> <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control"
                        TextMode="MultiLine" Rows="4" />
                </div>

                <div class="row g-3">
                    <div class="col-md-6 mb-3">
                        <label class="form-label"><%= Translate("category") %></label>
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select" />
                    </div>
                    <div class="col-md-6 mb-3">
                        <label class="form-label"><%= Translate("department") %></label>
                        <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-select" />
                    </div>
                </div>

                <div class="row g-3">
                    <div class="col-md-4 mb-3">
                        <label class="form-label"><%= Translate("incident_date") %></label>
                        <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label"><%= Translate("severity") %></label>
                        <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Low">Low</asp:ListItem>
                            <asp:ListItem Value="Medium">Medium</asp:ListItem>
                            <asp:ListItem Value="High">High</asp:ListItem>
                            <asp:ListItem Value="Critical">Critical</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="col-md-4 mb-3">
                        <label class="form-label"><%= Translate("status") %></label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Value="Open">Open</asp:ListItem>
                            <asp:ListItem Value="InProgress">In Progress</asp:ListItem>
                            <asp:ListItem Value="Closed">Closed</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="mb-3">
                    <label class="form-label"><%= Translate("location") %></label>
                    <asp:TextBox ID="txtLocation" runat="server" CssClass="form-control" MaxLength="200" />
                </div>

                <div class="mb-4">
                    <label class="form-label"><%= Translate("assigned_to") %></label>
                    <asp:DropDownList ID="ddlAssign" runat="server" CssClass="form-select" />
                </div>

                <div class="d-flex gap-2">
                    <asp:Button ID="btnSave" runat="server"
                        CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <a href="<%= ResolveUrl("~/Incidents/Details.aspx?id=" + IncidentId) %>"
                       class="btn btn-outline-secondary"><%= Translate("cancel") %></a>
                </div>

            </div>
        </div>
    </div>
</div>
</div>
</asp:Content>
