using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ContactDetailsApi.V2.Boundary.Request
{
    public class ServicesoftFetchContactDetailsRequest
    {
        [FromQuery]
        public string PaginationToken { get; set; }

        [FromQuery] [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = int.MaxValue;
    }
}
