using backend.Models;

namespace backend.Interfaces
{
    public interface ICalculatorDeductionsEmployeeService
    {
        decimal CalculateEmployeeDeductions(EmployeeCalculationDto empleado, int idReporte, long cedulaJuridicaEmpresa);
        List<EmployeeSocialSecurityContributions> ObtenerDeduccionesSocialesActuales();
        List<PersonalIncomeTax> ObtenerEscalasImpuestoRenta();
    }
}