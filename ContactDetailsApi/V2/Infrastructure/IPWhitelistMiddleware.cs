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
        private readonly byte[][] _safelist;

        public IPWhitelistMiddleware(
            RequestDelegate next,
            ILogger<IPWhitelistMiddleware> logger,
            string safelist)
        {
            safelist = Environment.GetEnvironmentVariable("WHITELIST_IP_ADDRESS");
            var ips = safelist.Split(';');
            _safelist = new byte[ips.Length][];
            for (var i = 0; i < ips.Length; i++)
            {
                _safelist[i] = IPAddress.Parse(ips[i]).GetAddressBytes();
            }

            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/api/v1/servicesoft/contactDetails")
            {
                var remoteIp = context.Connection.RemoteIpAddress;
                _logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);

                var bytes = remoteIp.GetAddressBytes();
                var badIp = true;
                foreach (var address in _safelist)
                {
                    if (address.SequenceEqual(bytes))
                    {
                        badIp = false;
                        break;
                    }
                }

                if (badIp)
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
