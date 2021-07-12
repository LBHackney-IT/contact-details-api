using Microsoft.AspNetCore.Mvc;
using System;

namespace ContactDetailsApi.V1.Boundary.Request
{
    public class DeleteContactQueryParameter
    {
        [FromQuery(Name = "targetId")]
        public Guid TargetId { get; set; }

        [FromQuery(Name = "id")]
        public Guid Id { get; set; }
    }
}
