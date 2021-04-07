using ContactDetailsApi.V1.Boundary.Response;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface IGetByIdUseCase
    {
        ResponseObject Execute(int id);
    }
}
