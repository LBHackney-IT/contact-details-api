using Hackney.Shared.Person.Infrastructure;
using Hackney.Shared.Tenure.Domain;
using System;
using System.Collections.Generic;

namespace ContactDetailsApi.V2.Infrastructure
{
    public class ContactByUprn
    {
        public string Uprn { get; set; }
        public Guid? TenureId { get; set; }
        public List<Person> Contacts { get; set; }

    }


    public class Person
    {
        public string PersonTenureType { get; set; }
        public bool IsResponsible { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }

        public List<PersonContactDetails> PersonContactDetails { get; set; }
    }

    public class PersonContactDetails
    {
        public string ContactType { get; set; }
        public string SubType { get; set; }
        public string Value { get; set; }

    }
}
