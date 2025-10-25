using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using backend.Interfaces.Repositories;
using backend.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class PayrollValidator : IPayrollValidator
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<PayrollValidator> _logger;

        public PayrollValidator(
            IPayrollRepository payrollRepository,
            IEmployeeService employeeService,
            ILogger<PayrollValidator> logger)
        {
            _payrollRepository = payrollRepository;
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task<PayrollValidationResult> ValidateAsync(PayrollProcessRequest request)
        {
            var existingPayroll = await _payrollRepository.GetByPeriodAndCompanyAsync(
                request.PeriodDate, request.CompanyId);

            if (existingPayroll?.IsCalculated == true)
            {
                var details = await _payrollRepository.GetPayrollDetailsAsync(existingPayroll.PayrollId);
                return PayrollValidationResult.AsError(existingPayroll, details);
            }

            var empleados = _employeeService.GetByEmpresa(request.CompanyId);
            if (!empleados.Any())
                return PayrollValidationResult.AsError("No hay empleados para procesar la planilla.");

            return PayrollValidationResult.AsValid();
        }
    }
}