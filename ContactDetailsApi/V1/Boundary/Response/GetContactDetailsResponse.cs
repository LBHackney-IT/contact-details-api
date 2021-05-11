using System.Collections.Generic;

namespace ContactDetailsApi.V1.Boundary.Response
{
    public class GetContactDetailsResponse
    {
        public List<ContactDetailsResponseObject> Results { get; set; } = new List<ContactDetailsResponseObject>();

        public GetContactDetailsResponse() { }
        public GetContactDetailsResponse(List<ContactDetailsResponseObject> results)
        {
            Results = results;
        }
    }
}
