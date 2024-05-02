using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests
{
    public class MockWebApplicationFactoryWithMiddleware<TStartup>
    : MockWebApplicationFactory<TStartup> where TStartup : class
    {


        public MockWebApplicationFactoryWithMiddleware() : base()
        {
            EnsureEnvVarConfigured("WHITELIST_IP_ADDRESS", "127.0.0.1");
        }


        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<MiddlewareConfigurationStartup>();

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<Action<IApplicationBuilder>>(app =>
                {
                    // Add middleware to the pipeline
                    app.UseMiddleware<OverrideIpAddressMiddleware>();
                });
            });
        }
    }

    /// Represents a startup class that configures middleware for the application.
    public class MiddlewareConfigurationStartup : Startup
    {
        public MiddlewareConfigurationStartup(IConfiguration configuration) : base(configuration)
        {
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var middlewareConfigurationAction = app.ApplicationServices.GetService<Action<IApplicationBuilder>>();
            middlewareConfigurationAction?.Invoke(app);

            base.Configure(app, env, logger);
        }
    }

    // OverrideIpAddressMiddleware is used to set the remote IP address to a fake value
    public class OverrideIpAddressMiddleware
    {
        private readonly RequestDelegate _next;

        public OverrideIpAddressMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            byte[] ipBytes = { 127, 0, 0, 1 };
            IPAddress ipAddress = new IPAddress(ipBytes);
            context.Connection.RemoteIpAddress = ipAddress;
            await _next(context);
        }
    }
}
