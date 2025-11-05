using backend.Interfaces;
using backend.Interfaces.Services;
using backend.Models;
using System.Linq;
using backend.Models.Payroll;
using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using Microsoft.Extensions.Logging;
using static backend.Controllers.PayrollController;

namespace backend.Services
{
    public class PayrollProcessingService : IPayrollProcessingService
    {
        private readonly IPayrollRepository _payrollRepository;
        private readonly ICalculationService _calculationService;
        private readonly IEmployeeService _employeeService;
        private readonly IPayrollValidator _payrollValidator;
        private readonly IPayrollResultBuilder _resultBuilder;
        private readonly IPayrollPeriodService _payrollPeriodService;
        private readonly ILogger<PayrollProcessingService> _logger;

        public PayrollProcessingService(
            IPayrollRepository payrollRepository,
            ICalculationService calculationService,
            IEmployeeService employeeService,
            IPayrollValidator payrollValidator,
            IPayrollResultBuilder resultBuilder,
            IPayrollPeriodService payrollPeriodService,
            ILogger<PayrollProcessingService> logger)
        {
            _payrollRepository = payrollRepository;
            _calculationService = calculationService;
            _employeeService = employeeService;
            _payrollValidator = payrollValidator;
            _resultBuilder = resultBuilder;
            _payrollPeriodService = payrollPeriodService;
            _logger = logger;
        }

        public async Task<List<PayrollPeriod>> GetPendingPeriodsAsync(string companyId, int months = 6)
        {
            try
            {
                _logger.LogInformation("Obteniendo periodos pendientes para compañía: {CompanyId}", companyId);

                var pendingPeriods = await _payrollPeriodService.GetPendingPeriodsAsync(companyId, months);

                _logger.LogInformation("Encontrados {Count} periodos pendientes para compañía: {CompanyId}",
                    pendingPeriods.Count, companyId);

                return pendingPeriods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo periodos pendientes para compañía: {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<List<PayrollPeriod>> GetOverduePeriodsAsync(string companyId)
        {
            try
            {
                _logger.LogInformation("Obteniendo periodos atrasados para compañía: {CompanyId}", companyId);

                var overduePeriods = await _payrollPeriodService.GetOverduePeriodsAsync(companyId);

                _logger.LogInformation("Encontrados {Count} periodos atrasados para compañía: {CompanyId}",
                    overduePeriods.Count, companyId);

                return overduePeriods;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo periodos atrasados para compañía: {CompanyId}", companyId);
                throw;
            }
        }
        public async Task<PayrollPeriod> GetNextPayrollPeriodAsync(string companyId)
        {
            try
            {
                _logger.LogInformation("Obteniendo próximo periodo de nómina para compañía: {CompanyId}", companyId);

                var nextPeriod = await _payrollPeriodService.CalculateNextPendingPeriodAsync(companyId);

                _logger.LogInformation("Próximo periodo: {Description} ({StartDate} - {EndDate})",
                    nextPeriod.Description, nextPeriod.StartDate.ToString("yyyy-MM-dd"),
                    nextPeriod.EndDate.ToString("yyyy-MM-dd"));

                return nextPeriod;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo próximo periodo para compañía: {CompanyId}", companyId);
                throw;
            }
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
                var today = DateTime.Now.Date;
                if (request.PeriodDate > today)
                {
                    _logger.LogWarning(
                        "Intento de procesar periodo futuro: {PeriodDate} (Hoy: {Today})",
                        request.PeriodDate.ToString("yyyy-MM-dd"),
                        today.ToString("yyyy-MM-dd"));

                    return _resultBuilder.CreateErrorResult(
                        $"No se pueden procesar nóminas para periodos futuros. " +
                        $"Solo se permiten periodos hasta: {today:yyyy-MM-dd}");
                }

                var minAllowedDate = DateTime.Now.AddYears(-1); 
                if (request.PeriodDate < minAllowedDate)
                {
                    _logger.LogWarning(
                        "Intento de procesar periodo muy pasado: {PeriodDate} (Límite: {MinDate})",
                        request.PeriodDate.ToString("yyyy-MM-dd"),
                        minAllowedDate.ToString("yyyy-MM-dd"));

                    return _resultBuilder.CreateErrorResult(
                        $"No se pueden procesar nóminas con más de 1 año de antigüedad. " +
                        $"Fecha mínima permitida: {minAllowedDate:yyyy-MM-dd}");
                }

                var isProcessed = await _payrollPeriodService.IsPeriodProcessedAsync(
                    request.CompanyId.ToString(), request.PeriodDate);

                if (isProcessed)
                {
                    _logger.LogWarning("El periodo {PeriodDate} ya está procesado para la compañía {CompanyId}",
                        request.PeriodDate.ToString("yyyy-MM-dd"), request.CompanyId);

                    return _resultBuilder.CreateErrorResult($"El periodo {request.PeriodDate:yyyy-MM-dd} ya fue procesado");
                }

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

           var totalEmployerCost = calculationResult.TotalGrossSalary + calculationResult.TotalEmployeeDeductions + calculationResult.TotalBenefits;

            var totalGrossSalary = calculationResult.EmployeeCalculations.Sum(x => x.Employee.salary);
            var totalEmployeeDeductions = calculationResult.TotalEmployeeDeductions;

            _logger.LogDebug(
                "COSTO EMPLEADOR - Bruto: {Gross}, Deducciones Empleador: {EmpDed}, Total: {Total}",
                totalGrossSalary, totalEmployeeDeductions, totalEmployerCost);

            return _resultBuilder.CreateSuccessResult(
                payroll.PayrollId,
                totalEmployerCost,  
                calculationResult.ProcessedEmployees);
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
            var resolvedPeriod = await _payrollPeriodService.ResolvePayrollPeriodAsync(request.CompanyId.ToString(), request.PeriodDate, allowProcessed: false);
            DateTime fechaInicio = request.PeriodDate;
            DateTime fechaFin = request.PeriodDate;

            if (resolvedPeriod != null)
            {
                fechaInicio = resolvedPeriod.StartDate;
                fechaFin = resolvedPeriod.EndDate;
            }
            else
            {
                _logger.LogWarning("No se pudo resolver un periodo para la fecha {PeriodDate} en la compañía {CompanyId}; se usará la fecha como inicio/fin", request.PeriodDate.ToString("yyyy-MM-dd"), request.CompanyId);
            }

            var employeeDtos = _employeeService.GetEmployeeCalculationDtos(request.CompanyId, fechaInicio, fechaFin);
            var result = new PayrollCalculationResult();

            _logger.LogInformation("Procesando {EmployeeCount} empleados (usando EmployeeCalculationDto)", employeeDtos.Count);

            foreach (var _dto in employeeDtos)
            {
                var reportedHours = _dto.HorasTrabajadas > 0 ? _dto.HorasTrabajadas : _dto.horas;
                _logger.LogDebug("Empleado DTO -> Cedula: {Cedula}, Nombre: {Nombre}, SalarioBruto: {Salario}, Horas: {Horas}, Tipo: {Tipo}",
                    _dto.CedulaEmpleado, _dto.NombreEmpleado, _dto.SalarioBruto, reportedHours, _dto.TipoEmpleado);

                if (_dto.SalarioBruto <= 0)
                {
                    try
                    {
                        var salarioFallback = await ObtenerSalarioEmpleado((int)_dto.CedulaEmpleado);
                        if (salarioFallback > 0)
                        {
                            _logger.LogInformation("Fallback: salario bruto actualizado desde DB para empleado {Cedula}: {Salario}", _dto.CedulaEmpleado, salarioFallback);
                            _dto.SalarioBruto = salarioFallback;
                        }
                        else
                        {
                            _logger.LogDebug("Fallback: no se encontró salario >0 para empleado {Cedula}", _dto.CedulaEmpleado);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error obteniendo salario por fallback para empleado {Cedula}", _dto.CedulaEmpleado);
                    }
                }
            }

            foreach (var empleadoDto in employeeDtos)
            {
                var periodType = resolvedPeriod?.PeriodType ?? backend.Models.Payroll.PayrollPeriodType.Mensual;
                if (!ValidateEmployeeHours(empleadoDto, periodType, out var invalidReason))
                {
                    _logger.LogInformation("Empleado omitido en cálculo - Cedula: {Cedula}, Nombre: {Nombre}, Motivo: {Reason}",
                        empleadoDto.CedulaEmpleado, empleadoDto.NombreEmpleado, invalidReason);
                    continue;
                }

                var deductions = await _calculationService.CalculateDeductionsAsync(empleadoDto, request.CompanyId, payrollId);
                var benefits = await _calculationService.CalculateBenefitsAsync(empleadoDto, request.CompanyId, payrollId);

                var empleadoModel = new EmpleadoModel
                {
                    id = (int)empleadoDto.CedulaEmpleado,
                    name = empleadoDto.NombreEmpleado,
                    salary = (int)empleadoDto.SalarioBruto,
                    employmentType = empleadoDto.TipoEmpleado,
                    department = "",
                    idCompny = request.CompanyId
                };

                result.AddEmployeeCalculation(new EmployeeCalculation(empleadoModel, deductions, benefits));
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
            _logger.LogInformation("=== DEBUG CALCULATION RESULTS ===");
            _logger.LogInformation("EmployeeCalculations count: {Count}", calculationResult.EmployeeCalculations.Count);

            var totalGrossSalary = calculationResult.EmployeeCalculations.Sum(x => x.Employee.salary);
            var totalBenefits = calculationResult.EmployeeCalculations.Sum(x => x.Benefits);
            var totalNetSalary = calculationResult.EmployeeCalculations.Sum(x => x.NetSalary);
            var totalEmployerDeductions = calculationResult.EmployeeCalculations.Sum(x => x.Deductions);

            _logger.LogInformation("Calculated - Gross: {Gross}, Benefits: {Benefits}, Net: {Net}, EmpDeductions: {EmpDed}",
                totalGrossSalary, totalBenefits, totalNetSalary, totalEmployerDeductions);

            var payments = calculationResult.ToPayments(payroll.PayrollId);
            await _payrollRepository.CreatePayrollPaymentsAsync(payments);

            var totalEmployeeDeductions = 0m;
            foreach (var employeeCalc in calculationResult.EmployeeCalculations)
            {
                var empleadoDto = MapToEmployeeDto(employeeCalc.Employee);
                var employerDeductions = await _calculationService.CalculateEmployerDeductionsAsync(
                    empleadoDto, payroll.CompanyId, payroll.PayrollId);
                totalEmployeeDeductions += employerDeductions;
            }

            payroll.IsCalculated = true;
            payroll.TotalGrossSalary = totalGrossSalary;
            payroll.TotalEmployeeDeductions = totalEmployeeDeductions;
            payroll.TotalEmployerDeductions = totalEmployerDeductions;
            payroll.TotalBenefits = totalBenefits;
            payroll.TotalNetSalary = totalNetSalary;
            payroll.TotalEmployerCost = totalGrossSalary + totalEmployeeDeductions + totalBenefits;
            payroll.LastModified = DateTime.Now;

            _logger.LogInformation("=== SAVING PAYROLL ===");
            _logger.LogInformation("Payroll {Id} - Gross: {Gross}, Net: {Net}, Benefits: {Benefits}",
                payroll.PayrollId, payroll.TotalGrossSalary, payroll.TotalNetSalary, payroll.TotalBenefits);

            await _payrollRepository.UpdatePayrollAsync(payroll);
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

        // No se pudo implementar validación de horas trabajadas al 100%
        private bool ValidateEmployeeHours(EmployeeCalculationDto dto, backend.Models.Payroll.PayrollPeriodType periodType, out string reason)
        {   
            reason = string.Empty;
            return true;
            if (dto == null) {
                reason = "DTO nulo";
                return false;
            }

            var hours = dto.HorasTrabajadas > 0 ? dto.HorasTrabajadas : dto.horas;

            var tipo = (dto.TipoEmpleado ?? string.Empty).ToLowerInvariant();

            var isMensual = periodType == backend.Models.Payroll.PayrollPeriodType.Mensual;
            var isQuincenal = periodType == backend.Models.Payroll.PayrollPeriodType.Quincenal;

            if (tipo.Contains("completo"))
            {
                if (isMensual)
                {
                    if (hours <= 45)
                    {
                        reason = $"Horas insuficientes: {hours}. Empleado 'Tiempo completo' (Mensual) requiere más de 45 horas por periodo.";
                        return false;
                    }
                }
                else if (isQuincenal)
                {
                    if (hours <= 22)
                    {
                        reason = $"Horas insuficientes: {hours}. Empleado 'Tiempo completo' (Quincenal) requiere más de 22 horas por periodo.";
                        return false;
                    }
                }
            }
            else if (tipo.Contains("medio"))
            {
                if (isMensual)
                {
                    if (hours <= 22)
                    {
                        reason = $"Horas insuficientes: {hours}. Empleado 'Medio tiempo' (Mensual) requiere más de 22 horas por periodo.";
                        return false;
                    }
                }
                else if (isQuincenal)
                {
                    if (hours <= 11)
                    {
                        reason = $"Horas insuficientes: {hours}. Empleado 'Medio tiempo' (Quincenal) requiere más de 11 horas por periodo.";
                        return false;
                    }
                }
            }

            return true;
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

        public async Task<PayrollProcessResult> PreviewPayrollAsync(PayrollPreviewRequest request)
        {
            using var activity = _logger.BeginScope("Preview de nómina para compañía {CompanyId}", request.CompanyId);

            try
            {
                _logger.LogInformation("Calculando PREVIEW para período: {PeriodDate}", request.PeriodDate.ToString("yyyy-MM-dd"));

                var today = DateTime.Now.Date;
                if (request.PeriodDate > today)
                {
                    return _resultBuilder.CreateErrorResult(
                        $"No se pueden previsualizar nóminas para periodos futuros. " +
                        $"Solo se permiten periodos hasta: {today:yyyy-MM-dd}");
                }

                var minAllowedDate = DateTime.Now.AddYears(-1);
                if (request.PeriodDate < minAllowedDate)
                {
                    return _resultBuilder.CreateErrorResult(
                        $"No se pueden previsualizar nóminas con más de 1 año de antigüedad. " +
                        $"Fecha mínima permitida: {minAllowedDate:yyyy-MM-dd}");
                }

                var calculationResult = await CalculatePayrollWithoutSaving(request);


                var totalEmployerCost = calculationResult.TotalGrossSalary + calculationResult.TotalEmployeeDeductions + calculationResult.TotalBenefits;

                return _resultBuilder.CreatePreviewResult(
                    totalEmployerCost,  
                    calculationResult.ProcessedEmployees,
                    calculationResult.TotalGrossSalary,
                    calculationResult.TotalEmployeeDeductions,
                    calculationResult.TotalEmployerDeductions,
                    calculationResult.TotalBenefits,
                    calculationResult.TotalNetSalary,
                    totalEmployerCost  
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en preview de nómina para compañía {CompanyId}", request.CompanyId);
                return _resultBuilder.CreateErrorResult($"Error en preview: {ex.Message}");
            }
        }

        private async Task<PayrollCalculationResult> CalculatePayrollWithoutSaving(PayrollPreviewRequest request)
        {
            var resolvedPeriod = await _payrollPeriodService.ResolvePayrollPeriodAsync(request.CompanyId.ToString(), request.PeriodDate, allowProcessed: true);
            DateTime fechaInicio = request.PeriodDate;
            DateTime fechaFin = request.PeriodDate;

            if (resolvedPeriod != null)
            {
                fechaInicio = resolvedPeriod.StartDate;
                fechaFin = resolvedPeriod.EndDate;
            }
            else
            {
                _logger.LogDebug("Preview: no se resolvió periodo para fecha {PeriodDate} en compañía {CompanyId}; usando fecha simple", request.PeriodDate.ToString("yyyy-MM-dd"), request.CompanyId);
            }

            var employeeDtos = _employeeService.GetEmployeeCalculationDtos(request.CompanyId, fechaInicio, fechaFin);
            var result = new PayrollCalculationResult();

            _logger.LogInformation("Calculando preview para {EmployeeCount} empleados", employeeDtos.Count);

            foreach (var _dto in employeeDtos)
            {
                var reportedHours = _dto.HorasTrabajadas > 0 ? _dto.HorasTrabajadas : _dto.horas;
                _logger.LogDebug("Preview DTO -> Cedula: {Cedula}, Nombre: {Nombre}, SalarioBruto: {Salario}, Horas: {Horas}, Tipo: {Tipo}",
                    _dto.CedulaEmpleado, _dto.NombreEmpleado, _dto.SalarioBruto, reportedHours, _dto.TipoEmpleado);

                if (_dto.SalarioBruto <= 0)
                {
                    try
                    {
                        var salarioFallback = await ObtenerSalarioEmpleado((int)_dto.CedulaEmpleado);
                        if (salarioFallback > 0)
                        {
                            _logger.LogInformation("Preview Fallback: salario bruto actualizado desde DB para empleado {Cedula}: {Salario}", _dto.CedulaEmpleado, salarioFallback);
                            _dto.SalarioBruto = salarioFallback;
                        }
                        else
                        {
                            _logger.LogDebug("Preview Fallback: no se encontró salario >0 para empleado {Cedula}", _dto.CedulaEmpleado);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Preview: Error obteniendo salario por fallback para empleado {Cedula}", _dto.CedulaEmpleado);
                    }
                }
            }

            foreach (var empleadoDto in employeeDtos)
            {
                    var previewPeriodType = resolvedPeriod?.PeriodType ?? backend.Models.Payroll.PayrollPeriodType.Mensual;
                    if (!ValidateEmployeeHours(empleadoDto, previewPeriodType, out var invalidReasonPreview))
                {
                    _logger.LogDebug("Preview: empleado {Cedula} ({Nombre}) omitido por horas insuficientes: {Reason}",
                        empleadoDto.CedulaEmpleado, empleadoDto.NombreEmpleado, invalidReasonPreview);
                    continue;
                }

                var deductions = await _calculationService.CalculateDeductionsAsync(empleadoDto, request.CompanyId, 0);
                var benefits = await _calculationService.CalculateBenefitsAsync(empleadoDto, request.CompanyId, 0);

                var empleadoModel = new EmpleadoModel
                {
                    id = (int)empleadoDto.CedulaEmpleado,
                    name = empleadoDto.NombreEmpleado,
                    salary = (int)empleadoDto.SalarioBruto,
                    employmentType = empleadoDto.TipoEmpleado,
                    department = "",
                    idCompny = request.CompanyId
                };

                result.AddEmployeeCalculation(new EmployeeCalculation(empleadoModel, deductions, benefits));
            }

               var totalEmployerDeductions = 0m;
                foreach (var employeeCalc in result.EmployeeCalculations)
                {
                    var empleadoDto = employeeDtos.FirstOrDefault(e => e.CedulaEmpleado == employeeCalc.Employee.id);
                    if (empleadoDto != null)
                    {
                        var employerDeductions = await _calculationService.CalculateEmployerDeductionsAsync(
                            empleadoDto, request.CompanyId, 0);
                        totalEmployerDeductions += employerDeductions;
                    }
                }

            var totalGrossSalary = result.EmployeeCalculations.Sum(x => x.Employee.salary);
            var totalBenefits = result.EmployeeCalculations.Sum(x => x.Benefits);
            var totalNetSalary = result.EmployeeCalculations.Sum(x => x.NetSalary);
            var totalEmployeeDeductions = result.EmployeeCalculations.Sum(x => x.Deductions);

            result.TotalGrossSalary = totalGrossSalary;
            result.TotalEmployeeDeductions = totalEmployeeDeductions;
            result.TotalEmployerDeductions = totalEmployerDeductions;
            result.TotalNetSalary = totalNetSalary;
            result.TotalEmployerCost = totalGrossSalary + totalEmployerDeductions + totalBenefits;
            _logger.LogInformation(
                "Preview calculado - Empleados: {EmployeeCount}, Bruto: {GrossSalary}, Neto: {NetSalary}",
                result.ProcessedEmployees, result.TotalGrossSalary, result.TotalNetSalary);

            return result;
        }
        
    }
}