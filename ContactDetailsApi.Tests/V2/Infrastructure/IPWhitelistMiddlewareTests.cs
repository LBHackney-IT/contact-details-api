using ContactDetailsApi.V2.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Hackney.Core.Testing.Shared;
using Xunit;

namespace ContactDetailsApi.Tests.V2.Infrastructure
{
    [Collection("LogCall collection")]
    public class IpWhitelistMiddlewareTests
    {
        private readonly Mock<ILogger<IPWhitelistMiddleware>> _logger;
        private readonly IPWhitelistMiddleware _classUnderTest;
        private readonly HttpContext _httpContext;
        private readonly string _allowedIpAddress = "127.0.0.1";
        private readonly string _requestPath = "/api/v2/servicesoft/contactDetails";
        public IpWhitelistMiddlewareTests()
        {
            Environment.SetEnvironmentVariable("WHITELIST_IP_ADDRESS", _allowedIpAddress);

            _logger = new Mock<ILogger<IPWhitelistMiddleware>>();
            _httpContext = new DefaultHttpContext();
            _classUnderTest = new IPWhitelistMiddleware(null, _logger.Object);
        }

        private void SetIpAddressAndRequestPath(string requestPath, string address)
        {
            _httpContext.Request.Path = requestPath;
            _httpContext.Connection.RemoteIpAddress =  IPAddress.Parse(address);
        }

        [Fact]
        public async Task ShouldGoThroughWhenIpAddressMatches()
        {
            SetIpAddressAndRequestPath(_requestPath, _allowedIpAddress);
            await _classUnderTest.Invoke(_httpContext).ConfigureAwait(false);

            _httpContext.Response.StatusCode.Should().Be((int) HttpStatusCode.OK);
            _logger.VerifyExact(LogLevel.Information, $"Request from Remote IP address: {_allowedIpAddress}", Times.Once());
        }

        [Fact]
        public async Task ShouldReturnForbiddenWhenIpAddressDoesNotMatch()
        {
            var notAllowedIpAddress = "19.49.204.165";
            SetIpAddressAndRequestPath(_requestPath, notAllowedIpAddress);
            await _classUnderTest.Invoke(_httpContext).ConfigureAwait(false);

            _httpContext.Response.StatusCode.Should().Be((int) HttpStatusCode.Forbidden);
            _logger.VerifyExact(LogLevel.Warning, $"Forbidden Request from Remote IP address: {notAllowedIpAddress}", Times.Once());
        }

        [Fact]
        public async Task WhenRequestPathIsNotEnabledShouldNotCheckIpAddress()
        {
            var otherRequestPath = "/api/v2/other";
            var otherIpAddress = "204.146.63.238";
            SetIpAddressAndRequestPath(otherRequestPath, otherIpAddress);
            await _classUnderTest.Invoke(_httpContext).ConfigureAwait(false);

            _httpContext.Response.StatusCode.Should().Be((int) HttpStatusCode.OK);
            _logger.VerifyExact(LogLevel.Information, $"Request from Remote IP address: {otherIpAddress}", Times.Never());
        }

    }
}
