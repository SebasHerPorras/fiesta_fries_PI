using backend.Models;

namespace backend.Interfaces.Services
{
    public interface IEmployeeService
    {
        List<EmpleadoListDto> GetByEmpresa(long cedulaJuridica);
        EmpleadoModel CreateEmpleadoWithPersonaAndUser(EmpleadoCreateRequest request);
        Task<decimal> GetSalarioBrutoAsync(int cedulaEmpleado);
        Task<bool> UpdateEmpleadoAsync(int id, EmpleadoUpdateDto dto);
        Task<EmpleadoUpdateDto?> GetEmpleadoPersonaByIdAsync(int id);
    }
}