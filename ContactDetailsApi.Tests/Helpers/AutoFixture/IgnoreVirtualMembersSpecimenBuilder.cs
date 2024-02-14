using AutoFixture.Kernel;
using System.Reflection;

namespace ContactDetailsApi.Tests.Helpers.AutoFixture
{
    public class IgnoreVirtualMembersSpecimenBuilder : ISpecimenBuilder
    {
        // https://www.jankowskimichal.pl/en/2017/02/speeding-up-autofixture/
        public object Create(object request, ISpecimenContext context)
        {
            var propertyInfo = request as PropertyInfo;
            if (propertyInfo == null)
            {
                return new NoSpecimen();
            }

            if (propertyInfo.GetGetMethod().IsVirtual)
            {
                return null;
            }

            return new NoSpecimen();
        }
    }
}
