using Microsoft.AspNetCore.Builder;

namespace ContactDetailsApi.V2.Infrastructure
{
    public static class IPWhitelistMiddlewareExtensions
    {
        public static IApplicationBuilder UseIPWhitelist(this
        IApplicationBuilder builder)
        {
            return builder.UseMiddleware<IPWhitelistMiddleware>();
        }
    }
}
