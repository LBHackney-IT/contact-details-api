using AspectInjector.Broker;
using ContactDetailsApi.V2.Domain;
using Hackney.Core.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly HashSet<string> _whitelist;
        private readonly HashSet<string> _enabledEndpoints = new HashSet<string> { "/api/v2/servicesoft/contactDetails" };

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

            await _next.Invoke(context);
        }
    }
}
