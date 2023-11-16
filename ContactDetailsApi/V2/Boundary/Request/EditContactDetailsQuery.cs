using Microsoft.AspNetCore.Mvc;
using System;

namespace ContactDetailsApi.V2.Boundary.Request
{
    public class EditContactDetailsQuery
    {
        [FromRoute(Name = "personId")]
        public Guid PersonId { get; set; }

        [FromRoute(Name = "contactDetailId")]
        public Guid ContactDetailId { get; set; }
    }
}
