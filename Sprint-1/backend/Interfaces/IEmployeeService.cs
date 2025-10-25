using backend.Models;

namespace backend.Interfaces.Services
{
    public interface IEmployeeService
    {
        List<EmpleadoListDto> GetByEmpresa(long cedulaJuridica);
        EmpleadoModel CreateEmpleadoWithPersonaAndUser(EmpleadoCreateRequest request);
    }
}