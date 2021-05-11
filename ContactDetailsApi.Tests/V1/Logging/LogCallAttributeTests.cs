using ContactDetailsApi.V1.Logging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ContactDetailsApi.Tests.V1.Logging
{
    public class LogCallAttributeTests
    {
        [Fact]
        public void DefaultConstructorTestSetsLogLevelTrace()
        {
            var sut = new LogCallAttribute();
            sut.Level.Should().Be(LogLevel.Trace);
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Error)]
        public void CustomConstructorTestSetsLogLevel(LogLevel level)
        {
            var sut = new LogCallAttribute(level);
            sut.Level.Should().Be(level);
        }
    }
}
