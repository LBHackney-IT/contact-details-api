using Hackney.Core.Testing.Shared;
using Xunit;

namespace ContactDetailsApi.Tests
{
    [CollectionDefinition("LogCall collection")]
    public class LogCallAspectFixtureCollection : ICollectionFixture<LogCallAspectFixture>
    { }
}
