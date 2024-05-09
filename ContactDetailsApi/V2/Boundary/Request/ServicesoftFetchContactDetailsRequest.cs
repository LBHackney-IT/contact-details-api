using System;
using Microsoft.AspNetCore.Mvc;

namespace ContactDetailsApi.V2.Boundary.Request
{
    public class ServicesoftFetchContactDetailsRequest
    {
        [FromQuery]
        public Guid? LastEvaluatedKey { get; set; }
    }
}
