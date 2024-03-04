using ContactDetailsApi.V2.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContactDetailsApi.V2.UseCase.Interfaces
{
    public interface IFetchAllContactDetailsByUprnUseCase
    {
        Task<List<ContactByUprn>> ExecuteAsync();
    }
}
