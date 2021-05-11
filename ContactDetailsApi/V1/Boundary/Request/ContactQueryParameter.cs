using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace ContactDetailsApi.V1.Boundary.Request
{
    public class ContactQueryParameter
    {
        [FromQuery(Name = "targetId")]
        [Required]
        public Guid? TargetId { get; set; }

        [FromQuery(Name = "includeHistoric")]
        public bool IncludeHistoric { get; set; }
    }
}
