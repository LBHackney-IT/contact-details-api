using ContactDetailsApi.V2.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Infrastructure
{
    public class IPWhitelistMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly IPWhitelistOptions _iPWhitelistOptions;
        private readonly ILogger<IPWhitelistMiddleware> _logger;
        public IPWhitelistMiddleware(RequestDelegate next,
        ILogger<IPWhitelistMiddleware> logger,
            IOptions<IPWhitelistOptions> applicationOptionsAccessor)
        {
            _iPWhitelistOptions = applicationOptionsAccessor.Value;
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/api/v2/servicesoft/contactDetails")
            {
                var ipAddress = context.Connection.RemoteIpAddress;
                List<string> whiteListIPList = _iPWhitelistOptions.Whitelist;
                var isIPWhitelisted = whiteListIPList
                                    .Where(ip => IPAddress.Parse(ip)
                                    .Equals(ipAddress))
                                    .Any();
                if (!isIPWhitelisted)
                {
                    _logger.LogWarning(
                    "Request from Remote IP address: {RemoteIp} is forbidden.", ipAddress);
                    context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    return;
                }
            }
            await _next.Invoke(context);
        }
    }
}
