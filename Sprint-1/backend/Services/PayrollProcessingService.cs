using backend.Models.Payroll;
using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using backend.Interfaces.Repositories;
using backend.Interfaces.Services;

namespace backend.Services
{
    public class PayrollProcessingService : IPayrollProcessingService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ICalculationService _calculationService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<PayrollProcessingService> _logger;

        public PayrollProcessingService(
            IPayrollRepository payrollRepository,
            ICalculationService calculationService,
            IEmployeeService employeeService,
            ILogger<PayrollProcessingService> logger)
        {
            _payrollRepository = payrollRepository;
            _calculationService = calculationService;
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task<PayrollProcessResult> ProcessPayrollAsync(PayrollProcessRequest request)
        {
            using var activity = _logger.BeginScope("Procesando nómina para compañía {CompanyId}", request.CompanyId);

            try
            {
                var validationResult = await ValidatePayrollProcessAsync(request);
                if (!validationResult.CanProcess)
                    return validationResult.ErrorResult!;

                var payroll = await CreatePayrollAsync(request);
                var processingResult = await ProcessEmployeesAsync(request, payroll.PayrollId);
                await SavePayrollResultsAsync(payroll, processingResult);

                return CreateSuccessResult(payroll.PayrollId, processingResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando nómina para compañía {CompanyId}", request.CompanyId);
                return CreateErrorResult($"Error interno: {ex.Message}");
            }
        }

        private async Task<PayrollValidationResult> ValidatePayrollProcessAsync(PayrollProcessRequest request)
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
            var result = new PayrollProcessingResult();

            _logger.LogDebug("Procesando {EmployeeCount} empleados", empleados.Count);

            foreach (var empleado in empleados)
            {
                var employeeResult = await ProcessSingleEmployeeAsync(employee, request.CompanyId, payrollId);
                result.AddEmployeeResult(employeeResult);
            }

            return result;
        }

        private async Task<EmployeeProcessingResult> ProcessSingleEmployeeAsync(
             EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            var deductions = await _calculationService.CalculateDeductionsAsync(empleado, companyId, payrollId);
            var benefits = await _calculationService.CalculateBenefitsAsync(empleado, companyId, payrollId);
            var tax = await _calculationService.CalculateIncomeTaxAsync(empleado, companyId, payrollId);
            var netSalary = empleado.SalarioBruto - deductions + benefits - tax;

            _logger.LogDebug(
                "Empleado {Nombre}: Bruto={SalarioBruto}, Deducciones={Deductions}, Beneficios={Benefits}, Tax={Tax}, Neto={Neto}",
                empleado.NombreEmpleado, empleado.SalarioBruto, deductions, benefits, tax, netSalary);

            return new EmployeeProcessingResult(empleado, deductions, benefits, tax, netSalary);
        }

        private async Task SavePayrollResultsAsync(Payroll payroll, PayrollProcessingResult processingResult)
        {
            var payments = processingResult.ToPayrollPayments(payroll.PayrollId);
            await _payrollRepository.CreatePayrollPaymentsAsync(payments);

            payroll.IsCalculated = true;
            payroll.TotalAmount = processingResult.TotalAmount;
            payroll.LastModified = DateTime.Now;

            await _payrollRepository.UpdatePayrollAsync(payroll);
            _logger.LogDebug("Planilla {PayrollId} guardada con total: {TotalAmount}", payroll.PayrollId, payroll.TotalAmount);
        }

        private PayrollProcessResult CreateSuccessResult(int payrollId, PayrollProcessingResult processingResult)
        {
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

        private PayrollProcessResult CreateErrorResult(string message)
        {
            return new PayrollProcessResult
            {
                Success = false,
                Message = message
            };
        }
    }

    
    public class PayrollProcessingResult
    {
        private readonly List<EmployeeProcessingResult> _employeeResults = new();

        public decimal TotalDeductions => _employeeResults.Sum(x => x.Deductions);
        public decimal TotalBenefits => _employeeResults.Sum(x => x.Benefits);
        public decimal TotalTax => _employeeResults.Sum(x => x.Tax);
        public decimal TotalAmount => TotalDeductions + TotalBenefits + TotalTax;
        public int ProcessedEmployees => _employeeResults.Count;

        public void AddEmployeeResult(EmployeeProcessingResult result) => _employeeResults.Add(result);

        public List<PayrollPayment> ToPayrollPayments(int payrollId)
        {
            return _employeeResults.Select(r => r.ToPayrollPayment(payrollId)).ToList();
        }
    }

    public record EmployeeProcessingResult(
        EmployeeCalculationDto Empleado,
        decimal Deductions,
        decimal Benefits,
        decimal Tax,
        decimal NetSalary)
    {
        public PayrollPayment ToPayrollPayment(int payrollId) => new()
        {
            PayrollId = payrollId,
            EmployeeId = (int)Empleado.CedulaEmpleado,
            GrossSalary = Empleado.SalarioBruto,
            DeductionsAmount = Deductions,
            BenefitsAmount = Benefits,
            NetSalary = NetSalary,
            PaymentDate = DateTime.Now,
            Status = "PROCESADO"
        };
    }
}