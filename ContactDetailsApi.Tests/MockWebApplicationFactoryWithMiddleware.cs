using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Bogus.DataSets;
using ContactDetailsApi.V1.Domain.Sns;
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
using Hackney.Core.JWT;
using Hackney.Core.Http;
using System.Linq;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IO;
using System.Reflection;
using ContactDetailsApi.Versioning;
using System.Configuration;
using ContactDetailsApi.V1.UseCase.Interfaces;
using ContactDetailsApi.V2.UseCase.Interfaces;
using ContactDetailsApi.V2.UseCase;
using ContactDetailsApi.V1.UseCase;
using ContactDetailsApi.V2.Gateways.Interfaces;
using ContactDetailsApi.V2.Gateways;
using System.Net.Security;

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
            Environment.SetEnvironmentVariable("WHITELIST_IP_ADDRESS", "127.0.0.1");
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
                services.AddCors();
                services.AddMvc();


                services.AddLogCallAspect();

                services.ConfigureDynamoDB();
                services.ConfigureDynamoDbFixture();

                services.ConfigureSns();
                services.ConfigureSnsFixture();

                var serviceProvider = services.BuildServiceProvider();

                DynamoDbFixture = serviceProvider.GetRequiredService<IDynamoDbFixture>();
                DynamoDbFixture.EnsureTablesExist(_tables);

                SnsFixture = serviceProvider.GetRequiredService<ISnsFixture>();


                SnsFixture.CreateSnsTopic<ContactDetailsSns>("contactdetails.fifo", "CONTACT_DETAILS_SNS_ARN");

                services.AddHealthChecks();
                services.AddSwaggerGen();

                RegisterUseCases(services);
                RegisterGateways(services);
                RegisterFactories(services);

                services.AddScoped<IEntityUpdater, EntityUpdater>();

                ConfigureHackneyCoreDI(services);

            });

            builder.Configure(app =>
            {
                app.UseMiddleware<OverrideIpAddressMiddleware>();
                app.UseIPWhitelist();

                app.UseCors(builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("x-correlation-id"));


                app.UseCorrelationId();
                app.UseLoggingScope();


                app.UseXRay("contact-details-api");
                app.EnableRequestBodyRewind();


                app.UseSwagger();
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    // SwaggerGen won't find controllers that are routed via this technique.
                    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

                    endpoints.MapHealthChecks("/api/v1/healthcheck/ping", new HealthCheckOptions()
                    {
                        ResponseWriter = HealthCheckResponseWriter.WriteResponse
                    });
                });

                app.UseLogCall();
            });

        }

        private static void ConfigureHackneyCoreDI(IServiceCollection services)
        {
            services.AddSnsGateway()
                .AddTokenFactory()
                .AddHttpContextWrapper();
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddScoped<ContactDetailsApi.V1.Gateways.Interfaces.IContactDetailsGateway, ContactDetailsApi.V1.Gateways.ContactDetailsDynamoDbGateway>();
            services.AddScoped<ContactDetailsApi.V2.Gateways.Interfaces.IContactDetailsGateway, ContactDetailsApi.V2.Gateways.ContactDetailsDynamoDbGateway>();
            services.AddScoped<ITenureDbGateway, TenureDbGateway>();
            services.AddScoped<IPersonDbGateway, PersonDbGateway>();
        }

        private static void RegisterUseCases(IServiceCollection services)
        {
            services.AddScoped<IFetchAllContactDetailsByUprnUseCase, FetchAllContactDetailsByUprnUseCase>();

            services.AddScoped<IDeleteContactDetailsByTargetIdUseCase, DeleteContactDetailsByTargetIdUseCase>();

            services.AddScoped<ContactDetailsApi.V1.UseCase.Interfaces.ICreateContactUseCase, ContactDetailsApi.V1.UseCase.CreateContactUseCase>();
            services.AddScoped<ContactDetailsApi.V2.UseCase.Interfaces.ICreateContactUseCase, ContactDetailsApi.V2.UseCase.CreateContactUseCase>();

            services.AddScoped<ContactDetailsApi.V1.UseCase.Interfaces.IGetContactDetailsByTargetIdUseCase, ContactDetailsApi.V1.UseCase.GetContactDetailsByTargetIdUseCase>();
            services.AddScoped<ContactDetailsApi.V2.UseCase.Interfaces.IGetContactDetailsByTargetIdUseCase, ContactDetailsApi.V2.UseCase.GetContactDetailsByTargetIdUseCase>();

            services.AddScoped<IEditContactDetailsUseCase, ContactDetailsApi.V2.UseCase.EditContactDetailsUseCase>();

        }

        private static void RegisterFactories(IServiceCollection services)
        {
            services.AddScoped<ContactDetailsApi.V1.Factories.Interfaces.ISnsFactory, ContactDetailsApi.V1.Factories.ContactDetailsSnsFactory>();
            services.AddScoped<ContactDetailsApi.V2.Factories.Interfaces.ISnsFactory, ContactDetailsApi.V2.Factories.ContactDetailsSnsFactory>();
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
            byte[] ipBytes = { 127, 0, 0, 1 };
            IPAddress ipAddress = new IPAddress(ipBytes);
            context.Connection.RemoteIpAddress = ipAddress;
            await _next(context);
        }
    }

}
