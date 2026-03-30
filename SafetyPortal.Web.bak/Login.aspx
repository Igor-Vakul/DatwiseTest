<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SafetyPortal.Web.Login" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>SafetyPortal – Login</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="<%= ResolveUrl("~/Content/Site.css") %>" rel="stylesheet" />
</head>
<body>
<div class="login-wrapper">
    <div class="login-card card shadow">
        <div class="card-body p-4 p-md-5">
            <div class="text-center mb-4">
                <i class="bi bi-shield-check text-primary" style="font-size:3rem"></i>
                <h3 class="mt-2 fw-bold">SafetyPortal</h3>
                <p class="text-muted small">Workplace Safety Management</p>
            </div>

            <% if (!string.IsNullOrEmpty(ErrorMessage)) { %>
            <div class="alert alert-danger alert-dismissible fade show py-2" role="alert">
                <i class="bi bi-exclamation-circle me-1"></i><%= ErrorMessage %>
                <button type="button" class="btn-close btn-sm" data-bs-dismiss="alert"></button>
            </div>
            <% } %>

            <form method="post" runat="server">
                <asp:HiddenField ID="hdnReturnUrl" runat="server" />

                <div class="mb-3">
                    <label class="form-label fw-semibold">Email address</label>
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control"
                            placeholder="you@datwise.local" TextMode="Email" />
                    </div>
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Password</label>
                    <div class="input-group">
                        <span class="input-group-text"><i class="bi bi-lock"></i></span>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control"
                            placeholder="••••••••" TextMode="Password" />
                    </div>
                </div>

                <asp:Button ID="btnLogin" runat="server" Text="Sign In"
                    CssClass="btn btn-primary w-100 py-2 fw-semibold"
                    OnClick="btnLogin_Click" />
            </form>

            <div class="mt-4 pt-3 border-top">
                <p class="text-muted small text-center mb-2">Demo accounts:</p>
                <div class="d-flex flex-column gap-1">
                    <code class="small text-center">admin@datwise.local / Admin123!</code>
                    <code class="small text-center">safety.manager@datwise.local / Safety123!</code>
                </div>
            </div>
        </div>
    </div>
</div>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
