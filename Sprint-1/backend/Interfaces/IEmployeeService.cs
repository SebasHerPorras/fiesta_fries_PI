using backend.Models;

namespace backend.Interfaces.Services
{
    public interface IEmployeeService
    {
        List<EmpleadoListDto> GetByEmpresa(long cedulaJuridica);
        EmpleadoModel CreateEmpleadoWithPersonaAndUser(EmpleadoCreateRequest request);
        Task<decimal> GetSalarioBrutoAsync(int cedulaEmpleado);
        List<EmployeeCalculationDto> GetEmployeeCalculationDtos(long cedulaJuridica, DateTime fechaInicio, DateTime fechaFin);
    }
}