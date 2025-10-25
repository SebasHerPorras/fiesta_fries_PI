using backend.Models.Payroll.Results;
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

        public PayrollProcessResult CreateSuccessResult(int payrollId, PayrollProcessingResult processingResult)
        {
            _logger.LogDebug("Creando resultado exitoso para planilla {PayrollId}", payrollId);

            return new PayrollProcessResult
            {
                Success = true,
                Message = "Cálculo de nómina completado.",
                PayrollId = payrollId,
                TotalAmount = processingResult.TotalAmount,
                ProcessedEmployees = processingResult.ProcessedEmployees,
                TotalDeductions = processingResult.TotalDeductions,
                TotalBenefits = processingResult.TotalBenefits,
                TotalTax = processingResult.TotalTax
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
    }
}