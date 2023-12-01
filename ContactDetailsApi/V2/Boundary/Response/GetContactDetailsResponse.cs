using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.Boundary.Response
{
    public class GetContactDetailsResponse
    {
        public List<ContactDetailsResponseObject> Results { get; set; }

        public GetContactDetailsResponse(List<ContactDetailsResponseObject> results)
        {
            Results = results;
        }
    }
}
