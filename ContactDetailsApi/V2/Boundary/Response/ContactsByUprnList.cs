using System.Collections.Generic;
using ContactDetailsApi.V2.Domain;

namespace ContactDetailsApi.V2.Boundary.Response
{
    public class ContactsByUprnList
    {
        public IEnumerable<ContactByUprn> Results { get; set; }
    }
}
