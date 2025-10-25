using backend.Models;
using backend.Models.Payroll;
using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using backend.Interfaces;
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
            var calculationResult = await ProcessEmployeesAsync(request, payroll.PayrollId); 
            await SavePayrollResultsAsync(payroll, calculationResult); 

            return _resultBuilder.CreateSuccessResult(payroll.PayrollId, calculationResult.TotalAmount, calculationResult.ProcessedEmployees);
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

        private async Task<PayrollCalculationResult> ProcessEmployeesAsync(PayrollProcessRequest request, int payrollId)
        {
            var empleados = _employeeService.GetByEmpresa(request.CompanyId);
            var result = new PayrollCalculationResult();

            _logger.LogInformation("Procesando {EmployeeCount} empleados", empleados.Count);

            foreach (var empleado in empleados)
            {
                var calculation = await ProcessSingleEmployeeAsync(empleado, request.CompanyId, payrollId);
                result.AddEmployeeCalculation(calculation);
            }

            return result;
        }

        private async Task<EmployeeCalculation> ProcessSingleEmployeeAsync( 
            EmpleadoListDto empleado, long companyId, int payrollId)
        {
            var empleadoDto = new EmployeeCalculationDto
            {
                CedulaEmpleado = empleado.Cedula,
                NombreEmpleado = empleado.Nombre,
                SalarioBruto = await ObtenerSalarioEmpleado(empleado.Cedula),
                TipoEmpleado = empleado.TipoContrato
            };

            var deductions = await _calculationService.CalculateDeductionsAsync(empleadoDto, companyId, payrollId);
            var benefits = await _calculationService.CalculateBenefitsAsync(empleadoDto, companyId, payrollId);
            var tax = await _calculationService.CalculateIncomeTaxAsync(empleadoDto, companyId, payrollId);

            var empleadoModel = new EmpleadoModel
            {
                id = empleado.Cedula,
                name = empleado.Nombre,
                salary = (int)empleadoDto.SalarioBruto,
                employmentType = empleado.TipoContrato,
                department = "",
                idCompny = companyId
            };

            _logger.LogDebug(
                "Empleado {Nombre} procesado - Bruto: {Bruto}",
                empleado.Nombre, empleadoDto.SalarioBruto);

            return new EmployeeCalculation(empleadoModel, deductions, benefits, tax);
        }

        private async Task SavePayrollResultsAsync(Payroll payroll, PayrollCalculationResult calculationResult)
        {
            var payments = calculationResult.ToPayments(payroll.PayrollId); 
            await _payrollRepository.CreatePayrollPaymentsAsync(payments);

            payroll.IsCalculated = true;
            payroll.TotalAmount = calculationResult.TotalAmount;
            payroll.LastModified = DateTime.Now;

            await _payrollRepository.UpdatePayrollAsync(payroll);

            _logger.LogInformation(
                "Planilla {PayrollId} guardada - Total: {TotalAmount}, Empleados: {EmployeeCount}",
                payroll.PayrollId, payroll.TotalAmount, calculationResult.ProcessedEmployees);
        }

        private async Task<decimal> ObtenerSalarioEmpleado(int cedula)
        {
            //Implementar para obtener salario real
            return 500000;
        }
    }
}