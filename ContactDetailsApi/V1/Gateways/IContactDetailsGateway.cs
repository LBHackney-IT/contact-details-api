using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContactDetailsApi.V1.Domain;
using ContactDetailsApi.V1.Infrastructure;

namespace ContactDetailsApi.V1.Gateways
{
    public interface IContactDetailsGateway
    {
        Task<List<ContactDetails>> GetContactByTargetId(Guid targetId);

    }
}
