using backend.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class EmployeeProcessor
    {
        private readonly ICalculationService _calculationService;
        private readonly ILogger<EmployeeProcessor> _logger;

        public EmployeeProcessor(
            ICalculationService calculationService,
            ILogger<EmployeeProcessor> logger)
        {
            _calculationService = calculationService;
            _logger = logger;
        }

        public async Task<PayrollProcessingResult> ProcessAllAsync(
            List<EmployeeCalculationDto> empleados, long companyId, int payrollId)
        {
            var result = new PayrollProcessingResult();

            _logger.LogDebug("Procesando {EmployeeCount} empleados", empleados.Count);

            foreach (var empleado in empleados)
            {
                var employeeResult = await ProcessSingleAsync(empleado, companyId, payrollId);
                result.AddEmployeeResult(employeeResult);
            }

            return result;
        }

        private async Task<EmployeeProcessingResult> ProcessSingleAsync(
            EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            var deductions = await _calculationService.CalculateDeductionsAsync(empleado, companyId, payrollId);
            var benefits = await _calculationService.CalculateBenefitsAsync(empleado, companyId, payrollId);
            var tax = await _calculationService.CalculateIncomeTaxAsync(empleado, companyId, payrollId);
            var netSalary = empleado.SalarioBruto - deductions + benefits - tax;

            _logger.LogDebug(
                "Empleado {Nombre} procesado - Bruto: {Bruto}, Neto: {Neto}",
                empleado.NombreEmpleado, empleado.SalarioBruto, netSalary);

            return new EmployeeProcessingResult(empleado, deductions, benefits, tax, netSalary);
        }
    }
}