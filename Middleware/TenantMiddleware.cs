using System.Security.Claims;

namespace LunchSystem.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId");
            if (tenantIdClaim != null)
            {
                context.Items["TenantId"] = int.Parse(tenantIdClaim.Value);
            }
        }

        await _next(context);
    }
}
