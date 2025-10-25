using backend.Models;

namespace backend.Interfaces.Services
{
    public interface IEmployeeService
    {
        List<EmployeeCalculationDto> GetByEmpresa(long cedulaJuridica);
    }
}