using ContactDetailsApi.V2.Boundary.Response;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase.Interfaces
{
    public interface IFetchAllContactDetailsByUprnUseCase
    {
        Task<ContactsByUprnList> ExecuteAsync();
    }
}
