using backend.Models.Payroll.Results;
using backend.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class PayrollResultBuilder : IPayrollResultBuilder
    {
        private readonly ILogger<PayrollResultBuilder> _logger;

        public PayrollResultBuilder(ILogger<PayrollResultBuilder> logger)
        {
            _logger = logger;
        }

        public PayrollProcessResult CreateSuccessResult(int payrollId, decimal totalAmount, int processedEmployees)
        {
            _logger.LogDebug("Creando resultado exitoso para planilla {PayrollId}", payrollId);

            return new PayrollProcessResult
            {
                Success = true,
                Message = $"Cálculo de nómina completado - Empleados: {processedEmployees}, Costo Total: ₡{totalAmount:N2}",
                PayrollId = payrollId,
                TotalAmount = totalAmount,
                ProcessedEmployees = processedEmployees,
                TotalDeductions = 0,
                TotalBenefits = 0,
                TotalTax = 0
            };
        }

        public PayrollProcessResult CreateErrorResult(string message)
        {
            _logger.LogDebug("Creando resultado de error: {ErrorMessage}", message);

            return new PayrollProcessResult
            {
                Success = false,
                Message = message
            };
        }
        public PayrollProcessResult CreatePreviewResult(
          decimal totalAmount,
          int processedEmployees,
          decimal totalGrossSalary,
          decimal totalEmployeeDeductions,
          decimal totalEmployerDeductions,
          decimal totalBenefits,
          decimal totalNetSalary,
          decimal totalEmployerCost)
        {
            return new PayrollProcessResult
            {
                Success = true,
                Message = $"Preview calculado - Empleados: {processedEmployees}, Total: ₡{totalAmount:N2}",
                ProcessedEmployees = processedEmployees,
                TotalAmount = totalAmount,
                PreviewData = new PreviewPayrollData
                {
                    TotalGrossSalary = totalGrossSalary,
                    TotalEmployeeDeductions = totalEmployeeDeductions,
                    TotalEmployerDeductions = totalEmployerDeductions,
                    TotalBenefits = totalBenefits,
                    TotalNetSalary = totalNetSalary,
                    TotalEmployerCost = totalEmployerCost
                }
            };
        }
    }
}