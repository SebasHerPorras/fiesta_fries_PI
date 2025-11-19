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
                Console.WriteLine("=== VALIDACIÓN DETALLADA DE BENEFICIOS ===");
                Console.WriteLine("Empleado: " + employee.NombreEmpleado);
                Console.WriteLine("Salario bruto: " + employee.SalarioBruto);
                Console.WriteLine("Cédula: " + employee.CedulaEmpleado);

                if (employee == null)
                    throw new ArgumentException("Los datos del empleado son requeridos");

                if (employee.SalarioBruto <= 0)
                    throw new ArgumentException("El salario bruto debe ser mayor a cero");

                var employeeBenefits = await _employeeBenefitService.GetSelectedByEmployeeIdAsync((int)employee.CedulaEmpleado);

                if (employeeBenefits == null || !employeeBenefits.Any())
                {
                    Console.WriteLine("No hay beneficios para este empleado");
                    return 0;
                }

                decimal totalEmployerCost = 0;
                var employerDeductions = new List<EmployerBenefitDeductionDto>();
                var employeeDeductions = new List<EmployeeDeductionsByPayrollDto>();

                Console.WriteLine("Cantidad de beneficios: " + employeeBenefits.Count);

                foreach (var employeeBenefit in employeeBenefits)
                {
                    Console.WriteLine("--- Procesando: " + employeeBenefit.ApiName + " ---");
                    Console.WriteLine("Tipo: " + employeeBenefit.BenefitType + ", Valor: " + employeeBenefit.BenefitValue);

                    var result = await ProcessEmployeeBenefitAsync(employeeBenefit, employee, reportId, cedulaJuridicaEmpresa);

                    Console.WriteLine("Resultado: " + result.employerAmount);

                    if (employeeBenefit.BenefitType == "PORCENTUAL" && employeeBenefit.BenefitValue.HasValue)
                    {
                        var calculoEsperado = employee.SalarioBruto * (employeeBenefit.BenefitValue.Value / 100);
                        Console.WriteLine("VALIDACIÓN PORCENTUAL: " + employeeBenefit.BenefitValue + "% de " + employee.SalarioBruto + " = " + calculoEsperado);
                        Console.WriteLine("COINCIDENCIA: " + (result.employerAmount == calculoEsperado));
                    }

                    totalEmployerCost += result.employerAmount;
                    Console.WriteLine("Acumulado: " + totalEmployerCost);

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

                Console.WriteLine("=== VALIDACIÓN FINAL ===");
                Console.WriteLine("Total beneficios: " + totalEmployerCost);
                var porcentaje = (totalEmployerCost / employee.SalarioBruto) * 100;
                Console.WriteLine("Porcentaje sobre salario: " + porcentaje + "%");

                if (totalEmployerCost > employee.SalarioBruto)
                {
                    Console.WriteLine("ADVERTENCIA: Beneficios superan el 100% del salario");
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
                Console.WriteLine("ERROR en validación: " + ex.Message);
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
                    Console.WriteLine("PASO API SIN PROBLEMAS");
                    break;

                default:
                    Console.WriteLine($"Tipo de beneficio no reconocido: {benefitType}");
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
                Console.WriteLine("CALLING API: " + apiName + " para " + employee.NombreEmpleado);

                var normalizedApiName = NormalizeApiName(apiName);
                Console.WriteLine("API normalizada: " + normalizedApiName);

                switch (normalizedApiName)
                {
                    case "asociacionsolidarista":
                        Console.WriteLine("API Reconocida: Asociación Solidarista");
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
                        Console.WriteLine("API Reconocida: Pensiones Voluntarias");
                        var pensionsService = _apiFactory.CreateVoluntaryPensionsService();
                        return await pensionsService.CalculatePremiumAsync(new VoluntaryPensionsRequest
                        {
                            PlanType = pensionType?.ToUpper() ?? "A",
                            GrossSalary = employee.SalarioBruto
                        });

                    default:
                        Console.WriteLine("API no reconocida: " + apiName);
                        Console.WriteLine("Buscando: '" + normalizedApiName + "'");
                        Console.WriteLine("Opciones disponibles: asociacionsolidarista, seguroprivado, pensionesvoluntarias");
                        return new ExternalApiResponse();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling external API " + apiName + ": " + ex.Message);
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