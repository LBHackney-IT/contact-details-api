using Microsoft.AspNetCore.Mvc;

namespace ContactDetailsApi.V2.Boundary.Request
{
    public class FetchAllContactDetailsQuery
    {
        [FromQuery]
        public string PaginationToken { get; set; }

        [FromQuery]
        public int? PageSize { get; set; }
    }
}
