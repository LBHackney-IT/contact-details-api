using Xunit;

namespace ContactDetailsApi.Tests
{
    [CollectionDefinition("AppTest middleware collection", DisableParallelization = true)]
    public class AppTestCollectionMiddleware : ICollectionFixture<MockWebApplicationFactoryWithMiddleware<Startup>>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
