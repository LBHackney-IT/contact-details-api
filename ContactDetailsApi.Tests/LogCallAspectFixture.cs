using ContactDetailsApi.V1.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;


namespace ContactDetailsApi.Tests
{
    public class LogCallAspectFixture : IDisposable
    {
        public Mock<ILogger<LogCallAspect>> MockLogger { get; private set; }

        [SetUp]
        public void RunBeforeTests()
        {
            MockLogger = SetupLogCallAspect();
        }

        private static Mock<ILogger<LogCallAspect>> SetupLogCallAspect()
        {
            var mockLogger = new Mock<ILogger<LogCallAspect>>();
            var mockAspect = new Mock<LogCallAspect>(mockLogger.Object);
            var mockAppServices = new Mock<IServiceProvider>();
            var appBuilder = new Mock<IApplicationBuilder>();

            appBuilder.SetupGet(x => x.ApplicationServices).Returns(mockAppServices.Object);
            LogCallAspectServices.UseLogCall(appBuilder.Object);
            mockAppServices.Setup(x => x.GetService(typeof(LogCallAspect))).Returns(mockAspect.Object);
            return mockLogger;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_disposed)
            {
                //foreach (var action in )
                //    action();

                _disposed = true;
            }
        }

    }
}
