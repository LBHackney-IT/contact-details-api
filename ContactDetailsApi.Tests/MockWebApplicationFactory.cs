using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bogus.DataSets;
using ContactDetailsApi.Tests.V2.Helper;
using ContactDetailsApi.V1.Domain.Sns;
using ContactDetailsApi.V2.Helper;
using ContactDetailsApi.V2.Infrastructure;
using ContactDetailsApi.V2.Infrastructure.Interfaces;
using Hackney.Core.DynamoDb;
using Hackney.Core.HealthCheck;
using Hackney.Core.Logging;
using Hackney.Core.Middleware.CorrelationId;
using Hackney.Core.Middleware.Exception;
using Hackney.Core.Middleware.Logging;
using Hackney.Core.Middleware;
using Hackney.Core.Sns;
using Hackney.Core.Testing.DynamoDb;
using Hackney.Core.Testing.Sns;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ContactDetailsApi.Tests
{
    public class MockWebApplicationFactory<TStartup>
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

        public MockWebApplicationFactory()
        {
            EnsureEnvVarConfigured("DynamoDb_LocalMode", "true");
            EnsureEnvVarConfigured("DynamoDb_LocalServiceUrl", "http://localhost:8000");
            EnsureEnvVarConfigured("Sns_LocalMode", "true");
            EnsureEnvVarConfigured("Localstack_SnsServiceUrl", "http://localhost:4566");
            Environment.SetEnvironmentVariable("WHITELIST_IP_ADDRESS", "123456");

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
                .UseStartup<Startup>();
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
            builder.Configure(app =>
            {

                //app.UseCors(builder => builder
                //    .AllowAnyOrigin()
                //    .AllowAnyHeader()
                //    .AllowAnyMethod()
                //    .WithExposedHeaders("x-correlation-id"));

                app.UseMiddleware<OverrideIpAddressMiddleware>();
                app.UseIPWhitelist();


                app.UseCorrelationId();
                app.UseLoggingScope();
                //app.UseCustomExceptionHandler(logger);

                //if (env.IsDevelopment())
                //{
                //    app.UseDeveloperExceptionPage();
                //}
                //else
                //{
                //    app.UseHsts();
                //}

                app.UseXRay("contact-details-api");
                app.EnableRequestBodyRewind();

               
                app.UseRouting();
               

                app.UseLogCall();
            });

        }
    }

    public class OverrideIpAddressMiddleware
    {
        private readonly RequestDelegate _next;

        public OverrideIpAddressMiddleware(RequestDelegate next)
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
