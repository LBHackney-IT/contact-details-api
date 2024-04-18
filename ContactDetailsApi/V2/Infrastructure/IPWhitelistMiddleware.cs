using ContactDetailsApi.V2.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
        private readonly ILogger<IPWhitelistMiddleware> _logger;
        private readonly HashSet<string> _safelist;

        public IPWhitelistMiddleware(
            RequestDelegate next,
            ILogger<IPWhitelistMiddleware> logger,
            string safelist)
        {
            try
            {
                safelist = Environment.GetEnvironmentVariable("WHITELIST_IP_ADDRESS");
                _logger.LogInformation("whitelist ip address is {safelist}", safelist);

            }
            catch(Exception ex)
            {
                _logger.LogInformation($" cannot get env var {ex.Message}");
            }

            var ips = safelist.Split(';');
            _safelist = new HashSet<string>(ips);

            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/api/v1/servicesoft/contactDetails")
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);

                if(!_safelist.Contains(remoteIp.ToString()))
                {

                    _logger.LogWarning(
                        "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                    context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                    return;
                }
                
                
            }

            await _next.Invoke(context);
        }
    }
}
