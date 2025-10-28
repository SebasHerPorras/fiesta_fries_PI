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
            ICalculatorDeductionsEmployerService employerDeductionsService,
            ILogger<PayrollProcessingService> logger)
        {
            _payrollRepository = payrollRepository;
            _calculationService = calculationService;
            _employeeService = employeeService;
            _payrollValidator = payrollValidator;
            _resultBuilder = resultBuilder;
            _logger = logger;
        }

        public async Task<List<PayrollSummaryResult>> GetPayrollsByCompanyAsync(string companyId)
        {
            _logger.LogInformation("Obteniendo planillas para compañía: {CompanyId}", companyId);

            try
            {
                var payrolls = await _payrollRepository.GetPayrollsByCompanyAsync(companyId);

                if (payrolls == null || !payrolls.Any())
                {
                    _logger.LogInformation("No se encontraron planillas para la compañía: {CompanyId}", companyId);
                    return new List<PayrollSummaryResult>();
                }

                var result = payrolls.Select(p => new PayrollSummaryResult
                {
                    PayrollId = p.PayrollId,
                    PeriodDate = p.PeriodDate,
                    CompanyId = p.CompanyId.ToString(),
                    IsCalculated = p.IsCalculated,
                    ApprovedBy = p.ApprovedBy,
                    LastModified = p.LastModified,
                    TotalGrossSalary = p.TotalGrossSalary ?? 0m,
                    TotalEmployerDeductions = p.TotalEmployerDeductions ?? 0m,
                    TotalEmployeeDeductions = p.TotalEmployeeDeductions ?? 0m,
                    TotalBenefits = p.TotalBenefits ?? 0m,
                    TotalNetSalary = p.TotalNetSalary ?? 0m,
                    TotalEmployerCost = p.TotalEmployerCost ?? 0m
                }).ToList();

                _logger.LogInformation("Se encontraron {Count} planillas para la compañía: {CompanyId}", result.Count, companyId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo planillas para compañía: {CompanyId}", companyId);
                throw;
            }
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

            return new EmployeeCalculation(empleadoModel, deductions, benefits);
        }

        private async Task SavePayrollResultsAsync(Payroll payroll, PayrollCalculationResult calculationResult)
        {
            var sumaDeducciones = calculationResult.EmployeeCalculations.Sum(x => x.Deductions);
            var payments = calculationResult.ToPayments(payroll.PayrollId);
            await _payrollRepository.CreatePayrollPaymentsAsync(payments);

            var totalEmployerDeductions = 0m;
            foreach (var employeeCalc in calculationResult.EmployeeCalculations)
            {
                var empleadoDto = MapToEmployeeDto(employeeCalc.Employee);
                var employerDeductions = await _calculationService.CalculateEmployerDeductionsAsync(
                    empleadoDto, payroll.CompanyId, payroll.PayrollId);

                totalEmployerDeductions += employerDeductions;
            }

            payroll.IsCalculated = true;
            payroll.TotalGrossSalary = calculationResult.EmployeeCalculations.Sum(x => x.Employee.salary);
            payroll.TotalEmployeeDeductions = totalEmployerDeductions;
            payroll.TotalEmployerDeductions = calculationResult.EmployeeCalculations.Sum(x => x.Deductions);
            payroll.TotalBenefits = calculationResult.TotalBenefits;
            payroll.TotalNetSalary = calculationResult.EmployeeCalculations.Sum(x => x.NetSalary);
            payroll.TotalEmployerCost = payroll.TotalGrossSalary + payroll.TotalEmployerDeductions;
            payroll.LastModified = DateTime.Now;

            await _payrollRepository.UpdatePayrollAsync(payroll);

            _logger.LogInformation(
                "Planilla {PayrollId} guardada - Total: {TotalAmount}, Empleados: {EmployeeCount}",
                payroll.PayrollId, payroll.TotalNetSalary, calculationResult.ProcessedEmployees);
        }

        private EmployeeCalculationDto MapToEmployeeDto(EmpleadoModel employee)
        {
            return new EmployeeCalculationDto
            {
                CedulaEmpleado = employee.id,
                NombreEmpleado = employee.name,
                SalarioBruto = employee.salary,
                TipoEmpleado = employee.employmentType,
            };
        }


        private async Task<decimal> ObtenerSalarioEmpleado(int cedula)
        {
            try
            {
                var salarioBruto = await _employeeService.GetSalarioBrutoAsync(cedula);

                _logger.LogDebug("Salario bruto obtenido: {Salario} para empleado {Cedula}",
                    salarioBruto, cedula);

                return salarioBruto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo salario para empleado {Cedula}", cedula);
                return 0;
            }
        }
    }
}