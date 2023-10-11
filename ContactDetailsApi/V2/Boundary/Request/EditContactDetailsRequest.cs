using ContactDetailsApi.V2.Domain;

namespace ContactDetailsApi.V2.Boundary.Request
{
    public class EditContactDetailsRequest
    {
        public ContactInformation ContactInformation { get; set; }
    }
}
