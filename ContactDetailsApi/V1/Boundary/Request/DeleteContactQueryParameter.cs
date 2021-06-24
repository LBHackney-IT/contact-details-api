using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
