using ContactDetailsApi.V1;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using Xunit;

namespace ContactDetailsApi.Tests.V1
{
    public class HttpHeadersExtensionsTests
    {
        private const string KEY = "someHeaderKey";
        private const string VALUE = "some value";
        private readonly Mock<IHeaderDictionary> _mockHeaders = new Mock<IHeaderDictionary>();
        delegate void SubmitCallback(string x, out StringValues y);

        [Fact]
        public void GetHeaderValueThrowsNullHeaders()
        {
            Func<string> func = () => HttpHeadersExtensions.GetHeaderValue(null, KEY);
            func.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetHeaderValueKeyNotFoundReturnsNull()
        {
            StringValues outVal;
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal)).Returns(false);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().BeNull();
        }

        [Fact]
        public void GetHeaderValueFounddNullKeyValue()
        {
            StringValues outVal;
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal)).Returns(true);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().BeNull();
        }

        [Fact]
        public void GetHeaderValueFoundEmptyKeyValue()
        {
            StringValues outVal = new StringValues("");
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal)).Returns(true);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().Be(string.Empty);
        }

        [Fact]
        public void GetHeaderValueFoundSingleKeyValue()
        {
            StringValues outVal;
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal))
                .Callback(new SubmitCallback((string x, out StringValues y) => y = new StringValues(VALUE)))
                .Returns(true);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().Be(VALUE);
        }

        [Fact]
        public void GetHeaderValueFoundManyKeyValuesReturnsFirst()
        {
            StringValues outVal;
            _mockHeaders.Setup(x => x.TryGetValue(KEY, out outVal))
                .Callback(new SubmitCallback((string x, out StringValues y) => y = new StringValues(new[] { VALUE, "val 2", "val 3" })))
                .Returns(true);
            _mockHeaders.Object.GetHeaderValue(KEY).Should().Be(VALUE);
        }

    }
}
