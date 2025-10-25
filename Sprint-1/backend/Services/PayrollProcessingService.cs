using backend.Models.Payroll;
using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using backend.Interfaces.Repositories;
using backend.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class PayrollProcessingService : IPayrollProcessingService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ICalculationService _calculationService;
        private readonly IEmployeeService _employeeService;
        private readonly IPayrollValidator _payrollValidator;
        private readonly IPayrollResultBuilder _resultBuilder;
        private readonly ILogger<PayrollProcessingService> _logger;

        public PayrollProcessingService(
            IPayrollRepository payrollRepository,
            ICalculationService calculationService,
            IEmployeeService employeeService,
            IPayrollValidator payrollValidator,
            IPayrollResultBuilder resultBuilder,
            ILogger<PayrollProcessingService> logger)
        {
            _payrollRepository = payrollRepository;
            _calculationService = calculationService;
            _employeeService = employeeService;
            _payrollValidator = payrollValidator;
            _resultBuilder = resultBuilder;
            _logger = logger;
        }

        public async Task<PayrollProcessResult> ProcessPayrollAsync(PayrollProcessRequest request)
        {
            using var activity = _logger.BeginScope("Procesando nómina para compañía {CompanyId}", request.CompanyId);

            try
            {
                return await ProcessPayrollInternalAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando nómina para compañía {CompanyId}", request.CompanyId);
                return _resultBuilder.CreateErrorResult($"Error interno: {ex.Message}");
            }
        }

        private async Task<PayrollProcessResult> ProcessPayrollInternalAsync(PayrollProcessRequest request)
        {
            var validationResult = await _payrollValidator.ValidateAsync(request);
            if (!validationResult.CanProcess)
                return validationResult.ErrorResult!;

            var payroll = await CreatePayrollAsync(request);
            var processingResult = await ProcessEmployeesAsync(request, payroll.PayrollId);
            await SavePayrollResultsAsync(payroll, processingResult);

            return _resultBuilder.CreateSuccessResult(payroll.PayrollId, processingResult);
        }

        private async Task<Payroll> CreatePayrollAsync(PayrollProcessRequest request)
        {
            var payroll = new Payroll
            {
                PeriodDate = request.PeriodDate,
                CompanyId = request.CompanyId,
                ApprovedBy = request.ApprovedBy,
                LastModified = DateTime.Now,
                IsCalculated = false
            };

            var payrollId = await _payrollRepository.CreatePayrollAsync(payroll);
            payroll.PayrollId = payrollId;

            _logger.LogDebug("Planilla creada con ID: {PayrollId}", payrollId);
            return payroll;
        }

        private async Task<PayrollProcessingResult> ProcessEmployeesAsync(PayrollProcessRequest request, int payrollId)
        {
            var empleados = _employeeService.GetByEmpresa(request.CompanyId);
            var processor = new EmployeeProcessor(_calculationService, _logger);

            return await processor.ProcessAllAsync(empleados, request.CompanyId, payrollId);
        }

        private async Task SavePayrollResultsAsync(Payroll payroll, PayrollProcessingResult processingResult)
        {
            var payments = processingResult.ToPayrollPayments(payroll.PayrollId);
            await _payrollRepository.CreatePayrollPaymentsAsync(payments);

            payroll.IsCalculated = true;
            payroll.TotalAmount = processingResult.TotalAmount;
            payroll.LastModified = DateTime.Now;

            await _payrollRepository.UpdatePayrollAsync(payroll);

            _logger.LogInformation(
                "Planilla {PayrollId} guardada - Total: {TotalAmount}, Empleados: {EmployeeCount}",
                payroll.PayrollId, payroll.TotalAmount, processingResult.ProcessedEmployees);
        }
    }
}