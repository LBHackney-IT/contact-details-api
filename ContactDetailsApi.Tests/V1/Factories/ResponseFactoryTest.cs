using AutoFixture;
using ContactDetailsApi.V1.Boundary.Response;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Factories;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;

namespace ContactDetailsApi.Tests.V1.Factories
{
    public class ResponseFactoryTest
    {
        //TODO: add assertions for all the fields being mapped in `ResponseFactory.ToResponse()`. Also be sure to add test cases for
        // any edge cases that might exist.
        [Test]
        public void CanMapADatabaseEntityToADomainObject()
        {
            var domain = new ContactDetails();
            var response = domain.ToResponse();
            //TODO: check here that all of the fields have been mapped correctly. i.e. response.fieldOne.Should().Be("one")
        }
    }
}
