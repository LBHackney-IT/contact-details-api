using ContactDetailsApi.V2.Boundary.Response;
using System.Threading.Tasks;
using ContactDetailsApi.V2.Boundary.Request;
using ContactDetailsApi.V2.Domain;
using Hackney.Core.DynamoDb;

namespace ContactDetailsApi.V2.UseCase.Interfaces
{
    public interface IFetchAllContactDetailsByUprnUseCase
    {
        Task<PagedResult<ContactByUprn>> ExecuteAsync(ServicesoftFetchContactDetailsRequest request);
    }
}
