using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hackney.Shared.Person;

namespace ContactDetailsApi.V2.Gateways.Interfaces;

public interface IPersonDbGateway
{
    Task<IEnumerable<Person>> GetPersons(List<Guid> ids);
}
