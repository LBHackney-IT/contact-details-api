using ContactDetailsApi.V1.Boundary.Response;

namespace ContactDetailsApi.V1.UseCase.Interfaces
{
    public interface IGetAllUseCase
    {
        ResponseObjectList Execute();
    }
}
