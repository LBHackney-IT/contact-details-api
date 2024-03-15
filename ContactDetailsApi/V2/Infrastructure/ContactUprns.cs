using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using System;
using System.Collections.Generic;
using ContactDetailsApi.V1.Domain;
using Hackney.Shared.Person.Domain;

namespace ContactDetailsApi.V2.Infrastructure
{
    public class ContactByUprn
    {
        public string Uprn { get; set; }
        public string Address { get; set; }
        public Guid? TenureId { get; set; }
        public List<Person> Contacts { get; set; }

    }


    public class Person
    {
        public PersonTenureType PersonTenureType { get; set; }
        public bool IsResponsible { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Title? Title { get; set; }

        public List<PersonContactDetails> PersonContactDetails { get; set; }
    }

    public class PersonContactDetails
    {
        public ContactType ContactType { get; set; }
        public SubType? SubType { get; set; }
        public string Value { get; set; }

    }
}
