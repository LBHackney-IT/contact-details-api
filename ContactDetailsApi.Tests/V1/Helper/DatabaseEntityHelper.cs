using AutoFixture;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using ContactDetailsApi.V1.Infrastructure;

namespace ContactDetailsApi.Tests.V1.Helper
{
    public static class DatabaseEntityHelper
    {
        public static ContactDetailsEntity CreateDatabaseEntity()
        {
            var entity = new Fixture().Create<ContactDetails>();

            return entity.ToDatabase();
        }
    }
}
