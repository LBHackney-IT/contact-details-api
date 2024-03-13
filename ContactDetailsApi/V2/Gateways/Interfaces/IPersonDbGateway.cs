using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hackney.Shared.Person.Infrastructure;

namespace ContactDetailsApi.V2.Gateways.Interfaces;

public interface IPersonDbGateway
{
    Task<IEnumerable<PersonDbEntity>> GetPersons(List<Guid> ids);
}
