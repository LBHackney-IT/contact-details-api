using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Boundary.Request
{
    public class ContactQueryParameter
    {
        [FromQuery(Name = "targetId")]
        [Required]
        public Guid TargetId { get; set; }

        [FromQuery(Name = "includeHistoric")]
        public string IncludeHistoric { get; set; }
    }
}
