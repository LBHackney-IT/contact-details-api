using Amazon.DynamoDBv2;
using ContactDetailsApi.V1.Domain.Sns;
using Hackney.Core.DynamoDb;
using Hackney.Core.Sns;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ContactDetailsApi.Tests
{
    public class MockWebApplicationFactoryWithMiddleware<TStartup>
    : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly List<TableDef> _tables = new List<TableDef>
        {
            new TableDef {
                Name = "ContactDetails",
                KeyName = "targetId",
                KeyType = ScalarAttributeType.S,
                RangeKeyName = "id",
                RangeKeyType = ScalarAttributeType.S
            },
            new TableDef { Name = "TenureInformation", KeyName = "id", KeyType = ScalarAttributeType.S },
            new TableDef { Name = "Persons", KeyName = "id", KeyType = ScalarAttributeType.S }

        };

        public HttpClient Client { get; private set; }
        public IDynamoDbFixture DynamoDbFixture { get; private set; }
        public ISnsFixture SnsFixture { get; private set; }

        public MockWebApplicationFactoryWithMiddleware()
        {
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");
            EnsureEnvVarConfigured("Sns_LocalMode", "true");
            EnsureEnvVarConfigured("Localstack_SnsServiceUrl", "http://localhost:4566");
            EnsureEnvVarConfigured("WHITELIST_IP_ADDRESS", "127.0.0.1");

            Client = CreateClient();
        }

        private bool _disposed;
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                if (null != DynamoDbFixture)
                    DynamoDbFixture.Dispose();
                if (null != SnsFixture)
                    SnsFixture.Dispose();
                if (null != Client)
                    Client.Dispose();

                base.Dispose(true);

                _disposed = true;
            }
        }

        private static void EnsureEnvVarConfigured(string name, string defaultValue)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(name)))
                Environment.SetEnvironmentVariable(name, defaultValue);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables())
                .UseStartup<StartupStub>();
            builder.ConfigureServices(services =>
            {
                services.ConfigureDynamoDB();
                services.ConfigureDynamoDbFixture();

                services.ConfigureSns();
                services.ConfigureSnsFixture();

                var serviceProvider = services.BuildServiceProvider();

                DynamoDbFixture = serviceProvider.GetRequiredService<IDynamoDbFixture>();
                DynamoDbFixture.EnsureTablesExist(_tables);

                SnsFixture = serviceProvider.GetRequiredService<ISnsFixture>();
                SnsFixture.CreateSnsTopic<ContactDetailsSns>("contactdetails.fifo", "CONTACT_DETAILS_SNS_ARN");
            });
        }
    }

    // StartupStub is used to add middleware to the pipeline
    public class StartupStub : Startup
    {
        public StartupStub(IConfiguration configuration) : base(configuration)
        {
        }

        public override void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseMiddleware<OverrideIpAddressMiddleware>();
            base.Configure(app, env, logger);
        }
    }

    // FakeRemoteIpAddressMiddleware is used to set the remote IP address to a fake value
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
