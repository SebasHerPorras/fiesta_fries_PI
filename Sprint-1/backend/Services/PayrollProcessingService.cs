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
        
        // Semáforo para evitar procesamiento concurrente de planillas por compañía
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _companyLocks = new();

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
            // 🔒 Obtener o crear semáforo para esta compañía
            var companyKey = $"{request.CompanyId}";
            var semaphore = _companyLocks.GetOrAdd(companyKey, _ => new SemaphoreSlim(1, 1));

            _logger.LogInformation("Esperando lock para compañía {CompanyId}...", request.CompanyId);
            await semaphore.WaitAsync();

            try
            {
                _logger.LogInformation("Lock obtenido para compañía {CompanyId}", request.CompanyId);                using var activity = _logger.BeginScope("Procesando nómina para compañía {CompanyId}", request.CompanyId);

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
            finally
            {
                semaphore.Release();
                _logger.LogInformation("Lock liberado para compañía {CompanyId}", request.CompanyId);
            }
        }

        private async Task<PayrollProcessResult> ProcessPayrollInternalAsync(PayrollProcessRequest request)
        {
            _logger.LogInformation("INICIANDO PROCESAMIENTO - Compañía: {CompanyId}, Periodo: {Period}", 
                request.CompanyId, request.PeriodDate.ToString("yyyy-MM-dd"));

            var validationResult = await _payrollValidator.ValidateAsync(request);
            if (!validationResult.CanProcess)
                return validationResult.ErrorResult!;

            var payroll = await CreatePayrollAsync(request);
            
            _logger.LogInformation("Payroll creado - PayrollId: {PayrollId} para periodo {Period}", 
                payroll.PayrollId, request.PeriodDate.ToString("yyyy-MM-dd"));
            
            // USAR EL MISMO MÉTODO QUE EL PREVIEW, pero guardando en BD
            var calculationResult = await CalculatePayrollAsync(
                request.CompanyId, 
                request.PeriodDate, 
                payroll.PayrollId,  // PayrollId > 0 indica que SE DEBE GUARDAR
                allowProcessed: false
            );

            _logger.LogInformation("Guardando resultados - PayrollId: {PayrollId}, Empleados procesados: {Count}", 
                payroll.PayrollId, calculationResult.ProcessedEmployees);

            await SavePayrollResultsAsync(payroll, calculationResult);

            var totalEmployerCost = calculationResult.TotalGrossSalary + calculationResult.TotalEmployerDeductions + calculationResult.TotalBenefits;

            _logger.LogInformation(
                "PLANILLA COMPLETADA - PayrollId: {PayrollId}, Bruto: ₡{Gross}, DeduccionesEmpleador: ₡{EmpDed}, CostoTotal: ₡{Total}",
                payroll.PayrollId, calculationResult.TotalGrossSalary, calculationResult.TotalEmployerDeductions, totalEmployerCost);

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

        // MÉTODO ELIMINADO: Ahora se usa CalculatePayrollAsync unificado

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
            _logger.LogInformation("=== GUARDANDO RESULTADOS DE PLANILLA ===");
            _logger.LogInformation("EmployeeCalculations count: {Count}", calculationResult.EmployeeCalculations.Count);

            // DEBUG: Verificar cada empleado individual
            foreach (var empCalc in calculationResult.EmployeeCalculations)
            {
                _logger.LogInformation("  {Nombre}: Salario=₡{Sal}, Deducciones=₡{Ded}, Beneficios=₡{Ben}, Neto=₡{Net}",
                    empCalc.Employee.name, empCalc.Employee.salary, empCalc.Deductions, empCalc.Benefits, empCalc.NetSalary);
            }

            // USAR LOS VALORES YA CALCULADOS EN ProcessEmployeesAsync
            var totalGrossSalary = calculationResult.TotalGrossSalary;
            var totalBenefits = calculationResult.TotalBenefits;
            var totalNetSalary = calculationResult.TotalNetSalary;
            var totalEmployeeDeductions = calculationResult.TotalEmployeeDeductions;
            var totalEmployerDeductions = calculationResult.TotalEmployerDeductions;

            _logger.LogInformation("Totales desde calculationResult properties:");
            _logger.LogInformation("   TotalGrossSalary: ₡{Gross}", totalGrossSalary);
            _logger.LogInformation("   TotalBenefits: ₡{Benefits}", totalBenefits);
            _logger.LogInformation("   TotalNetSalary: ₡{Net}", totalNetSalary);
            _logger.LogInformation("   TotalEmployeeDeductions: ₡{EmpDed}", totalEmployeeDeductions);
            _logger.LogInformation("   TotalEmployerDeductions: ₡{EmplrDed}", totalEmployerDeductions);

            // GUARDAR PAGOS INDIVIDUALES POR EMPLEADO
            var payments = calculationResult.ToPayments(payroll.PayrollId);
            await _payrollRepository.CreatePayrollPaymentsAsync(payments);

            // ACTUALIZAR PAYROLL CON LOS TOTALES YA CALCULADOS
            payroll.IsCalculated = true;
            payroll.TotalGrossSalary = totalGrossSalary;
            payroll.TotalEmployeeDeductions = totalEmployeeDeductions;
            payroll.TotalEmployerDeductions = totalEmployerDeductions;
            payroll.TotalBenefits = totalBenefits;
            payroll.TotalNetSalary = totalNetSalary;
            payroll.TotalEmployerCost = totalGrossSalary + totalEmployerDeductions + totalBenefits;
            payroll.LastModified = DateTime.Now;

            _logger.LogInformation("=== PAYROLL ACTUALIZADO ===");
            _logger.LogInformation("Payroll {Id} - Bruto: ₡{Gross}, Neto: ₡{Net}, Beneficios: ₡{Benefits}, Costo Empleador: ₡{Cost}",
                payroll.PayrollId, payroll.TotalGrossSalary, payroll.TotalNetSalary, payroll.TotalBenefits, payroll.TotalEmployerCost);

            await _payrollRepository.UpdatePayrollAsync(payroll);
        }

        private bool ValidateEmployeeHours(EmployeeCalculationDto dto, backend.Models.Payroll.PayrollPeriodType periodType, out string reason)
        {   
            reason = string.Empty;
            if (dto == null) {
                reason = "DTO nulo";
                return false;
            }

            var hours = dto.horas;

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

                // USAR EL MISMO MÉTODO, con payrollId = 0 (no guardar)
                var calculationResult = await CalculatePayrollAsync(
                    request.CompanyId,
                    request.PeriodDate,
                    payrollId: 0,  // 0 = PREVIEW (no guarda en BD)
                    allowProcessed: true
                );

                var totalEmployerCost = calculationResult.TotalGrossSalary
                                        + calculationResult.TotalEmployerDeductions
                                        + calculationResult.TotalBenefits;

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

        /// <summary>
        /// Método único para calcular planillas (preview O definitivo)
        /// </summary>
        /// <param name="companyId">ID de la empresa</param>
        /// <param name="periodDate">Fecha del periodo</param>
        /// <param name="payrollId">ID de planilla (0 = preview, >0 = guardar en BD)</param>
        /// <param name="allowProcessed">Permitir periodos ya procesados (solo para preview)</param>
        private async Task<PayrollCalculationResult> CalculatePayrollAsync(
            long companyId, 
            DateTime periodDate, 
            int payrollId, 
            bool allowProcessed)
        {
            var isPreview = payrollId == 0;
            var logPrefix = isPreview ? "PREVIEW" : "PROCESO";

            var resolvedPeriod = await _payrollPeriodService.ResolvePayrollPeriodAsync(
                companyId.ToString(), 
                periodDate, 
                allowProcessed);

            DateTime fechaInicio = periodDate;
            DateTime fechaFin = periodDate;

            if (resolvedPeriod != null)
            {
                fechaInicio = resolvedPeriod.StartDate;
                fechaFin = resolvedPeriod.EndDate;
                _logger.LogInformation("{Prefix}: Periodo resuelto {Start} - {End}", 
                    logPrefix, fechaInicio.ToString("yyyy-MM-dd"), fechaFin.ToString("yyyy-MM-dd"));
            }
            else
            {
                _logger.LogWarning("{Prefix}: No se resolvió periodo para {Date}, usando fecha simple", 
                    logPrefix, periodDate.ToString("yyyy-MM-dd"));
            }

            var employeeDtos = _employeeService.GetEmployeeCalculationDtos(companyId, fechaInicio, fechaFin);
            var result = new PayrollCalculationResult();

            _logger.LogInformation("{Prefix}: Calculando para {EmployeeCount} empleados", logPrefix, employeeDtos.Count);

            // Fallback de salarios
            foreach (var _dto in employeeDtos)
            {
                _logger.LogDebug("{Prefix} DTO -> {Cedula}: {Nombre}, Salario: ₡{Salario}, Horas: {Horas}, Tipo: {Tipo}",
                    logPrefix, _dto.CedulaEmpleado, _dto.NombreEmpleado, _dto.SalarioBruto, _dto.horas, _dto.TipoEmpleado);

                if (_dto.SalarioBruto <= 0)
                {
                    try
                    {
                        var salarioFallback = await ObtenerSalarioEmpleado((int)_dto.CedulaEmpleado);
                        if (salarioFallback > 0)
                        {
                            _logger.LogInformation("{Prefix} Fallback: Salario actualizado para {Cedula}: ₡{Salario}", 
                                logPrefix, _dto.CedulaEmpleado, salarioFallback);
                            _dto.SalarioBruto = salarioFallback;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "{Prefix}: Error en fallback de salario para {Cedula}", logPrefix, _dto.CedulaEmpleado);
                    }
                }
            }

            // Cálculo por empleado
            foreach (var empleadoDto in employeeDtos)
            {
                var periodType = resolvedPeriod?.PeriodType ?? backend.Models.Payroll.PayrollPeriodType.Mensual;
                if (!ValidateEmployeeHours(empleadoDto, periodType, out var invalidReason))
                {
                    _logger.LogInformation("{Prefix}: Empleado {Cedula} ({Nombre}) omitido - {Reason}",
                        logPrefix, empleadoDto.CedulaEmpleado, empleadoDto.NombreEmpleado, invalidReason);
                    continue;
                }

                // CALCULAR: Deducciones empleado y beneficios
                _logger.LogDebug("{Prefix}: Calculando para empleado {Cedula} con PayrollId: {PayrollId}",
                    logPrefix, empleadoDto.CedulaEmpleado, payrollId);
                
                var deductions = await _calculationService.CalculateDeductionsAsync(empleadoDto, companyId, payrollId);
                var benefits = await _calculationService.CalculateBenefitsAsync(empleadoDto, companyId, payrollId);

                _logger.LogInformation("{Prefix} - Empleado {Cedula}: Deducciones=₡{Ded}, Beneficios=₡{Ben}, Salario=₡{Sal}",
                    logPrefix, empleadoDto.CedulaEmpleado, deductions, benefits, empleadoDto.SalarioBruto);

                var empleadoModel = new EmpleadoModel
                {
                    id = (int)empleadoDto.CedulaEmpleado,
                    name = empleadoDto.NombreEmpleado,
                    salary = (int)empleadoDto.SalarioBruto,
                    employmentType = empleadoDto.TipoEmpleado,
                    department = "",
                    idCompny = companyId
                };

                result.AddEmployeeCalculation(new EmployeeCalculation(empleadoModel, deductions, benefits));
            }

            // CALCULAR: Deducciones del empleador (cargas sociales)
            var totalEmployerDeductions = 0m;
            foreach (var employeeCalc in result.EmployeeCalculations)
            {
                var empleadoDto = employeeDtos.FirstOrDefault(e => e.CedulaEmpleado == employeeCalc.Employee.id);
                if (empleadoDto != null)
                {
                    var employerDeductions = await _calculationService.CalculateEmployerDeductionsAsync(
                        empleadoDto, companyId, payrollId);
                    totalEmployerDeductions += employerDeductions;
                }
            }

            // ACTUALIZAR TOTALES
            var totalGrossSalary = result.EmployeeCalculations.Sum(x => x.Employee.salary);
            var totalBenefits = result.EmployeeCalculations.Sum(x => x.Benefits);
            var totalNetSalary = result.EmployeeCalculations.Sum(x => x.NetSalary);
            var totalEmployeeDeductions = result.EmployeeCalculations.Sum(x => x.Deductions);

            _logger.LogInformation("{Prefix} - TOTALES CALCULADOS:", logPrefix);
            _logger.LogInformation("   Bruto: ₡{Gross}", totalGrossSalary);
            _logger.LogInformation("   Beneficios: ₡{Benefits}", totalBenefits);
            _logger.LogInformation("   DeduccionesEmpleado: ₡{EmpDed}", totalEmployeeDeductions);
            _logger.LogInformation("   DeduccionesEmpleador: ₡{EmplrDed}", totalEmployerDeductions);
            _logger.LogInformation("   Neto: ₡{Net}", totalNetSalary);

            result.TotalGrossSalary = totalGrossSalary;
            result.TotalEmployeeDeductions = totalEmployeeDeductions;
            result.TotalEmployerDeductions = totalEmployerDeductions;
            result.TotalNetSalary = totalNetSalary;
            result.TotalEmployerCost = totalGrossSalary + totalEmployerDeductions + totalBenefits;

            _logger.LogInformation(
                "{Prefix} CALCULADO - Empleados: {Count}, Bruto: ₡{Gross}, DeduccionesEmpleador: ₡{EmpDed}, Neto: ₡{Net}",
                logPrefix, result.ProcessedEmployees, result.TotalGrossSalary, totalEmployerDeductions, result.TotalNetSalary);

            return result;
        }
        
    }
}
