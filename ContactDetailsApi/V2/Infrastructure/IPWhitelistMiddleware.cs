using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Infrastructure
{
    public class IPWhitelistMiddleware
    {

        public readonly RequestDelegate _next;
        public readonly ILogger<IPWhitelistMiddleware> _logger;
        public readonly HashSet<string> _whitelist;
        public readonly HashSet<string> _enabledEndpoints = new HashSet<string> { "/api/v2/servicesoft/contactDetails" };

        public IPWhitelistMiddleware(
            RequestDelegate next,
            ILogger<IPWhitelistMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            try
            {
                var whitelist = Environment.GetEnvironmentVariable("WHITELIST_IP_ADDRESS");
                _logger.LogInformation("whitelist ip address is {whitelist}", whitelist);
                var ips = whitelist.Split(';');
                _whitelist = new HashSet<string>(ips);

            }
            catch (Exception ex)
            {
                _logger.LogInformation($" cannot get env var {ex.Message}");
            }
        }

        public async Task Invoke(HttpContext context)
        {
            if (_enabledEndpoints.Contains(context.Request.Path))
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                _logger.LogInformation("Request from Remote IP address: {RemoteIp}", remoteIp);

                if (!_whitelist.Contains(remoteIp.ToString()))
                {
                    _logger.LogWarning(
                        "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                    context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    return;
                }
            }
            if (_next != null)
                await _next.Invoke(context);
        }
    }
}
