using System;
using Microsoft.AspNetCore.Mvc;

namespace ContactDetailsApi.V2.Boundary.Request
{
    public class ServicesoftFetchContactDetailsRequest
    {
        [FromQuery]
        public string PaginationToken { get; set; }

        [FromQuery]
        public int? PageSize { get; set; }
    }
}
