using ContactDetailsApi.V1.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;


namespace ContactDetailsApi.Tests.V1.Logging
{
    [TestFixture]
    public class LogCallAttributeTests
    {
        [Test]
        public void DefaultConstructorTestSetsLogLevelTrace()
        {
            var sut = new LogCallAttribute();
            sut.Level.Should().Be(LogLevel.Trace);
        }

    }
}
