using backend.Models;
using backend.Interfaces;

namespace backend.Services
{
    public class CalculatorBenefitsService : ICalculatorBenefitsService
    {
        private readonly IEmployeeDeductionsByPayrollService _employeeDeductionService;
        private readonly IEmployerBenefitDeductionService _employerBenefitDeductionService;
        private readonly IExternalApiFactory _apiFactory;
        private readonly IEmployeeBenefitService _employeeBenefitService;

        public CalculatorBenefitsService(
            IEmployeeDeductionsByPayrollService employeeDeductionService,
            IEmployerBenefitDeductionService employerBenefitDeductionService,
            IExternalApiFactory apiFactory,
            IEmployeeBenefitService employeeBenefitService)
        {
            _employeeDeductionService = employeeDeductionService;
            _employerBenefitDeductionService = employerBenefitDeductionService;
            _apiFactory = apiFactory;
            _employeeBenefitService = employeeBenefitService;
        }

        public async Task<decimal> CalculateBenefitsAsync(EmployeeCalculationDto employee, int reportId, long cedulaJuridicaEmpresa)
        {
            try
            {
                _logger.LogInformation("=== VALIDACIÓN DETALLADA DE BENEFICIOS ===");
                _logger.LogInformation("Empleado: {NombreEmpleado}", employee.NombreEmpleado);
                _logger.LogInformation("Salario bruto: {SalarioBruto}", employee.SalarioBruto);
                _logger.LogInformation("Cédula: {CedulaEmpleado}", employee.CedulaEmpleado);

                if (employee == null)
                    throw new ArgumentException("Los datos del empleado son requeridos");

                if (employee.SalarioBruto <= 0)
                    throw new ArgumentException("El salario bruto debe ser mayor a cero");

                var employeeBenefits = await _employeeBenefitService.GetSelectedByEmployeeIdAsync((int)employee.CedulaEmpleado);

                if (employeeBenefits == null || !employeeBenefits.Any())
                {
                    _logger.LogInformation("No hay beneficios para este empleado");
                    return 0;
                }

                decimal totalEmployerCost = 0;
                var employerDeductions = new List<EmployerBenefitDeductionDto>();
                var employeeDeductions = new List<EmployeeDeductionsByPayrollDto>();

                _logger.LogInformation("Cantidad de beneficios: {BenefitCount}", employeeBenefits.Count);

                foreach (var employeeBenefit in employeeBenefits)
                {
                    _logger.LogInformation("--- Procesando: {ApiName} ---", employeeBenefit.ApiName);
                    _logger.LogInformation("Tipo: {BenefitType}, Valor: {BenefitValue}", employeeBenefit.BenefitType, employeeBenefit.BenefitValue);

                    var result = await ProcessEmployeeBenefitAsync(employeeBenefit, employee, reportId, cedulaJuridicaEmpresa);

                    _logger.LogInformation("Resultado: {EmployerAmount}", result.employerAmount);

                    if (employeeBenefit.BenefitType == "PORCENTUAL" && employeeBenefit.BenefitValue.HasValue)
                    {
                        var calculoEsperado = employee.SalarioBruto * (employeeBenefit.BenefitValue.Value / 100);
                        _logger.LogInformation("VALIDACIÓN PORCENTUAL: {Percentage}% de {SalarioBruto} = {CalculoEsperado}", employeeBenefit.BenefitValue, employee.SalarioBruto, calculoEsperado);
                        _logger.LogInformation("COINCIDENCIA: {Coincide}", result.employerAmount == calculoEsperado);
                    }

                    totalEmployerCost += result.employerAmount;
                    _logger.LogInformation("Acumulado: {TotalAcumulado}", totalEmployerCost);

                    if (result.employerAmount > 0)
                    {
                        employerDeductions.Add(new EmployerBenefitDeductionDto
                        {
                            ReportId = reportId,
                            EmployeeId = (int)employee.CedulaEmpleado,
                            CedulaJuridicaEmpresa = cedulaJuridicaEmpresa,
                            BenefitName = employeeBenefit.ApiName ?? "Beneficio",
                            DeductionAmount = result.employerAmount,
                            BenefitType = employeeBenefit.BenefitType ?? "Unknown",
                            Percentage = employeeBenefit.BenefitType == "Porcentual" ? employeeBenefit.BenefitValue : null
                        });
                    }

                    if (result.employeeDeductions.Any())
                    {
                        employeeDeductions.AddRange(result.employeeDeductions);
                    }
                }

                _logger.LogInformation("=== VALIDACIÓN FINAL ===");
                _logger.LogInformation("Total beneficios: {TotalBeneficios}", totalEmployerCost);
                var porcentaje = (totalEmployerCost / employee.SalarioBruto) * 100;
                _logger.LogInformation("Porcentaje sobre salario: {Porcentaje}%", porcentaje);

                if (totalEmployerCost > employee.SalarioBruto)
                {
                    _logger.LogWarning("ADVERTENCIA: Beneficios superan el 100% del salario");
                }

                if (employeeDeductions.Any())
                {
                    _employeeDeductionService.SaveEmployeeDeductions(employeeDeductions);
                }

                if (employerDeductions.Any())
                {
                    _employerBenefitDeductionService.SaveEmployerBenefitDeductions(employerDeductions);
                }

                return Math.Round(totalEmployerCost, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR en validación de beneficios para empleado {CedulaEmpleado}", employee?.CedulaEmpleado);
                return 0;
            }
        }

        private async Task<(decimal employerAmount, List<EmployeeDeductionsByPayrollDto> employeeDeductions)> 
            ProcessEmployeeBenefitAsync(EmployeeBenefit employeeBenefit, EmployeeCalculationDto employee, int reportId, long cedulaJuridicaEmpresa)
        {
            decimal employerAmount = 0;
            var employeeDeductions = new List<EmployeeDeductionsByPayrollDto>();

            var benefitType = employeeBenefit.BenefitType?.ToUpperInvariant() ?? "UNKNOWN";

            switch (benefitType)
            {
                case "MONTO FIJO":
                    employerAmount = employeeBenefit.BenefitValue ?? 0;
                    break;

                case "PORCENTUAL":
                    if (employeeBenefit.BenefitValue.HasValue)
                    {
                        employerAmount = Math.Round(employee.SalarioBruto * (employeeBenefit.BenefitValue.Value / 100), 2);
                    }
                    break;

                case "API":
                    var apiResult = await CallExternalApiAsync(
                        employeeBenefit.ApiName ?? string.Empty, 
                        employee,
                        cedulaJuridicaEmpresa,
                        employeeBenefit.PensionType.ToString(),
                        employeeBenefit.DependentsCount);
                    
                    foreach (var deduction in apiResult.Deductions ?? new List<ApiDeduction>())
                    {
                        if (deduction.Type == "ER") 
                        {
                            employerAmount += deduction.Amount;
                        }
                        else if (deduction.Type == "EE") 
                        {
                            employeeDeductions.Add(new EmployeeDeductionsByPayrollDto
                            {
                                ReportId = reportId,
                                EmployeeId = (int)employee.CedulaEmpleado,
                                CedulaJuridicaEmpresa = cedulaJuridicaEmpresa,
                                DeductionName = $"Beneficio: {employeeBenefit.ApiName} (Empleado)",
                                DeductionAmount = deduction.Amount,
                                Percentage = null
                            });
                        }
                    }
                    _logger.LogInformation("PASO API SIN PROBLEMAS");
                    break;

                default:
                    _logger.LogWarning("Tipo de beneficio no reconocido: {BenefitType}", benefitType);
                    break;
            }

            return (employerAmount, employeeDeductions);
        }

        private async Task<ExternalApiResponse> CallExternalApiAsync(
             string apiName,
             EmployeeCalculationDto employee,
             long idCompany,
             string? pensionType = null,
             int? dependentsCount = null)
        {
            try
            {
                _logger.LogInformation("CALLING API: {ApiName} para {NombreEmpleado}", apiName, employee.NombreEmpleado);

                var normalizedApiName = NormalizeApiName(apiName);
                _logger.LogInformation("API normalizada: {NormalizedApiName}", normalizedApiName);

                switch (normalizedApiName)
                {
                    case "asociacionsolidarista":
                        _logger.LogInformation("API Reconocida: Asociación Solidarista");
                        var solidarityService = _apiFactory.CreateSolidarityAssociationService();
                        return await solidarityService.CalculateContributionAsync(new SolidarityAssociationRequest
                        {
                            CompanyLegalId = idCompany.ToString(),
                            GrossSalary = employee.SalarioBruto
                        });

                    case "seguroprivado":
                        Console.WriteLine("API Reconocida: Seguro Privado");
                        var insuranceService = _apiFactory.CreatePrivateInsuranceService();
                        return await insuranceService.CalculatePremiumAsync(new PrivateInsuranceRequest
                        {
                            BirthDate = employee.Cumpleanos,
                            Age = 0,
                            DependentsCount = dependentsCount ?? 0
                        });

                    case "pensionesvoluntarias":
                        _logger.LogInformation("API Reconocida: Pensiones Voluntaria
                        var pensionsService = _apiFactory.CreateVoluntaryPensionsService();
                        return await pensionsService.CalculatePremiumAsync(new VoluntaryPensionsRequest
                        {
                            PlanType = pensionType?.ToUpper() ?? "A",
                            GrossSalary = employee.SalarioBruto
                        });

                    default:
                        _logger.LogWarning("API no reconocida: {ApiName}", apiName);
                        _logger.LogInformation("Buscando: '{NormalizedApiName}'", normalizedApiName);
                        _logger.LogInformation("Opciones disponibles: asociacionsolidarista, seguroprivado, pensionesvoluntarias");
                        return new ExternalApiResponse();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling external API {ApiName}", apiName);
                return new ExternalApiResponse();
            }
        }

        private string NormalizeApiName(string apiName)
        {
            if (string.IsNullOrEmpty(apiName))
                return string.Empty;

            var normalized = apiName.ToLowerInvariant().Replace(" ", "");
            normalized = normalized
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");

            return normalized;
        }

        public List<BenefitDto> GetBenefitsList(long cedulaJuridicaEmpresa)
        {
            return new List<BenefitDto>();
        }
    }
}
