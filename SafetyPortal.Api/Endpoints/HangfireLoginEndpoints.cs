using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafetyPortal.Api.Data;
using SafetyPortal.Api.Entities;
using SafetyPortal.Api.Services;
using static SafetyPortal.Api.AppConstants;

namespace SafetyPortal.Api.Endpoints;

public static class HangfireLoginEndpoints
{
    private const string CookieName    = "HangfireAuth";
    private const string AdminRoleName = RoleNames.Admin;

    public static IEndpointRouteBuilder MapHangfireLoginEndpoints(this IEndpointRouteBuilder app)
    {
        // GET /jobs/login — show login form
        app.MapGet("/jobs/login", (HttpContext http) =>
        {
            var error = http.Request.Query.ContainsKey("error")
                ? "<p style='color:#dc3545;margin-bottom:1rem;'>Invalid email or password.</p>"
                : string.Empty;

            return Results.Content(LoginHtml(error), "text/html");
        });

        // POST /jobs/login — validate credentials, set cookie, redirect
        app.MapPost("/jobs/login", async (
            HttpContext          http,
            SafetyPortalDbContext db,
            JwtTokenService      jwtService) =>
        {
            var form     = await http.Request.ReadFormAsync();
            var email    = form["email"].ToString().Trim();
            var password = form["password"].ToString();

            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user is null || user.Role.Name != AdminRoleName)
                return Results.Redirect("/jobs/login?error=1");

            var hasher = new PasswordHasher<User>();
            if (hasher.VerifyHashedPassword(user, user.PasswordHash, password)
                    == PasswordVerificationResult.Failed)
                return Results.Redirect("/jobs/login?error=1");

            var token = jwtService.CreateToken(user);

            http.Response.Cookies.Append(CookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure   = false,   // set true when behind HTTPS
                SameSite = SameSiteMode.Strict,
                Path     = "/hangfire"
            });

            return Results.Redirect("/hangfire");
        });

        // GET /jobs/logout — clear cookie, redirect to login
        app.MapGet("/jobs/logout", (HttpContext http) =>
        {
            http.Response.Cookies.Delete(CookieName, new CookieOptions { Path = "/hangfire" });
            return Results.Redirect("/jobs/login");
        });

        return app;
    }

    // ── Minimal HTML login form ────────────────────────────────────────────────
    private static string LoginHtml(string errorHtml) => $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8"/>
          <meta name="viewport" content="width=device-width,initial-scale=1"/>
          <title>Hangfire — Admin Login</title>
          <style>
            *, *::before, *::after { box-sizing: border-box; }
            body {
              margin: 0; display: flex; align-items: center; justify-content: center;
              min-height: 100vh; background: #f0f2f5;
              font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
            }
            .card {
              background: #fff; border-radius: 8px; padding: 2.5rem 2rem;
              box-shadow: 0 4px 24px rgba(0,0,0,.08); width: 100%; max-width: 380px;
            }
            h2 { margin: 0 0 1.5rem; font-size: 1.25rem; color: #212529; }
            label { display: block; font-size: .875rem; color: #495057; margin-bottom: .25rem; }
            input {
              width: 100%; padding: .5rem .75rem; font-size: 1rem;
              border: 1px solid #ced4da; border-radius: 4px; margin-bottom: 1rem;
            }
            input:focus { outline: none; border-color: #0d6efd; box-shadow: 0 0 0 3px rgba(13,110,253,.15); }
            button {
              width: 100%; padding: .6rem; font-size: 1rem; font-weight: 500;
              background: #0d6efd; color: #fff; border: none; border-radius: 4px; cursor: pointer;
            }
            button:hover { background: #0b5ed7; }
          </style>
        </head>
        <body>
          <div class="card">
            <h2>Hangfire Dashboard</h2>
            {{errorHtml}}
            <form method="post" action="/jobs/login">
              <label for="email">Email</label>
              <input id="email" name="email" type="email" autocomplete="username" required/>
              <label for="password">Password</label>
              <input id="password" name="password" type="password" autocomplete="current-password" required/>
              <button type="submit">Sign in</button>
            </form>
          </div>
        </body>
        </html>
        """;
}
