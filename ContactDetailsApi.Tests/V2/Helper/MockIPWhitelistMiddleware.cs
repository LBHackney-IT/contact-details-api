using ContactDetailsApi.V2.Helper;
using ContactDetailsApi.V2.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests.V2.Helper
{
    public class MockIPWhitelistMiddleware 
    {
        private readonly RequestDelegate _next;

        public MockIPWhitelistMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            context.Connection.RemoteIpAddress = new IPAddress(123456);
            await _next(context);
        }
    }
}
