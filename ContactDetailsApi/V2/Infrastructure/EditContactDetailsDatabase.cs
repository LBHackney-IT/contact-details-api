using ContactDetailsApi.V2.Domain;
using System;

namespace ContactDetailsApi.V2.Infrastructure
{
    public class EditContactDetailsDatabase
    {
        public ContactInformation ContactInformation { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
