using ContactDetailsApi.V2.Infrastructure;

namespace ContactDetailsApi.V2.Domain
{
    public class EditContactDetailsDomain
    {
        public UpdateEntityResult<ContactDetailsEntity> UpdateResult { get; set; }
        public ContactDetailsEntity ExistingEntity { get; set; }
    }
}
