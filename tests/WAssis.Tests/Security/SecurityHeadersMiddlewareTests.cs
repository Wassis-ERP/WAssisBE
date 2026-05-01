using Microsoft.AspNetCore.Http;
using WAssis.Services.Api.Infrastructure;

namespace WAssis.Tests.Security;

public sealed class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldApplySecurityHeaders()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context);

        SecurityHeadersMiddleware.ApplyHeaders(context.Response.Headers);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
        Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"]);
        Assert.Equal("camera=(), microphone=(), geolocation=()", context.Response.Headers["Permissions-Policy"]);
        Assert.Equal("none", context.Response.Headers["X-Permitted-Cross-Domain-Policies"]);
        Assert.Contains("default-src 'none'", context.Response.Headers.ContentSecurityPolicy.ToString());
        Assert.Contains("frame-ancestors 'none'", context.Response.Headers.ContentSecurityPolicy.ToString());
    }
}
