using Amazon.Runtime.Internal.Util;
using ContactDetailsApi.V2.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace ContactDetailsApi.Tests.V2.Infrastructure
{
    [Collection("LogCall collection")]
    public class IPWhitelistMiddlewareTests
    {
        private readonly Mock<ILogger<IPWhitelistMiddleware>> _logger;
        private readonly IPWhitelistMiddleware _classUnderTest;
        private readonly HttpContext _stubHttpContext;
#pragma warning disable CS0649 // Field 'IPWhitelistMiddlewareTests._next' is never assigned to, and will always have its default value null
        private readonly RequestDelegate _next;
#pragma warning restore CS0649 // Field 'IPWhitelistMiddlewareTests._next' is never assigned to, and will always have its default value null

        public IPWhitelistMiddlewareTests()
        {
            _logger = new Mock<ILogger<IPWhitelistMiddleware>>();
            _stubHttpContext = new DefaultHttpContext();
            _classUnderTest = new IPWhitelistMiddleware(_next, _logger.Object);
            Environment.SetEnvironmentVariable("WHITELIST_IP_ADDRESS", "127.0.0.1");
        }

        private async Task SetIPAddress()
        {
            byte[] ipBytes = { 127, 0, 0, 1 };
            IPAddress ipAddress = new IPAddress(ipBytes);
            _stubHttpContext.Connection.RemoteIpAddress = ipAddress;
            await _next(_stubHttpContext);
        }

        [Fact]
        public async Task ShouldGoThroughWhenIPAddressMatches()
        {
            await SetIPAddress().ConfigureAwait(false);
            await _classUnderTest.Invoke(_stubHttpContext).ConfigureAwait(false);


        }

        [Fact]
        public void ShouldReturnForbiddenWhenIPAddressDoesNotMatch()
        {

        }

    }
}
