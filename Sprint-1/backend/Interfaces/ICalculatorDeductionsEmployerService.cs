using backend.Models;

namespace backend.Interfaces
{
    public interface ICalculatorDeductionsEmployerService
    {
        decimal CalculateEmployerDeductions(EmployeeCalculationDto empleado, int idReporte, long cedulaJuridicaEmpresa);
        List<EmployerSocialSecurityContributions> ObtenerCargasSocialesActuales();
    }
}