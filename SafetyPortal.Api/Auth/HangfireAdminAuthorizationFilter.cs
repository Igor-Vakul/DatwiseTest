using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.IdentityModel.Tokens;

namespace SafetyPortal.Api.Auth;

/// <summary>
/// Allows access to the Hangfire dashboard only for users with a valid Admin JWT
/// stored in the HangfireAuth HttpOnly cookie.
/// </summary>
public class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
{
    private const string CookieName = "HangfireAuth";

    private readonly JwtOptions _jwt;

    public HangfireAdminAuthorizationFilter(JwtOptions jwt)
    {
        _jwt = jwt;
    }

    public bool Authorize(DashboardContext context)
    {
        var http = context.GetHttpContext();
        var token = http.Request.Cookies[CookieName];

        if (string.IsNullOrEmpty(token))
        {
            // Redirect browser to login page instead of returning a raw 401
            http.Response.Redirect("/jobs/login");
            return false;
        }

        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            new JwtSecurityTokenHandler().ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,
                    ValidateIssuer           = true,
                    ValidIssuer              = _jwt.Issuer,
                    ValidateAudience         = true,
                    ValidAudience            = _jwt.Audience,
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                },
                out var validated);

            var jwt  = (JwtSecurityToken)validated;
            var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role == "Admin")
                return true;

            // Authenticated but not Admin — show 403
            http.Response.StatusCode = StatusCodes.Status403Forbidden;
            return false;
        }
        catch
        {
            // Token expired or invalid — redirect to login
            http.Response.Redirect("/jobs/login");
            return false;
        }
    }
}
