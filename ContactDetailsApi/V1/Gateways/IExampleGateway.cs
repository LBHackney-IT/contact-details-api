using System.Collections.Generic;
using ContactDetailsApi.V1.Domain;

namespace ContactDetailsApi.V1.Gateways
{
    public interface IExampleGateway
    {
        Entity GetEntityById(int id);

        List<Entity> GetAll();
    }
}
