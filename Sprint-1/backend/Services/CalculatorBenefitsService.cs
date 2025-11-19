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
            if (employee == null)
                throw new ArgumentException("Los datos del empleado son requeridos");

            if (employee.SalarioBruto <= 0)
                throw new ArgumentException("El salario bruto debe ser mayor a cero");

            // USAR GetSelectedByEmployeeIdAsync para obtener beneficios del empleado
            var employeeBenefits = await _employeeBenefitService.GetSelectedByEmployeeIdAsync((int)employee.CedulaEmpleado);

            if (employeeBenefits == null || !employeeBenefits.Any())
            {
                return 0; // No hay beneficios para este empleado
            }

            decimal totalEmployerCost = 0;
            var employerDeductions = new List<EmployerBenefitDeductionDto>(); 
            var employeeDeductions = new List<EmployeeDeductionsByPayrollDto>(); 

            foreach (var employeeBenefit in employeeBenefits)
            {
                var result = await ProcessEmployeeBenefitAsync(employeeBenefit, employee, reportId, cedulaJuridicaEmpresa);
                
                // Acumular costos del empleador
                totalEmployerCost += result.employerAmount;

                // Agregar a lista de deducciones del empleador
                if (result.employerAmount > 0)
                {
                    employerDeductions.Add(new EmployerBenefitDeductionDto
                    {   
                        ReportId = reportId,
                        EmployeeId = (int)employee.CedulaEmpleado,
                        CedulaJuridicaEmpresa = cedulaJuridicaEmpresa,
                        BenefitName = employeeBenefit.ApiName ?? "Beneficio",
                        BenefitId = employeeBenefit.BenefitId,
                        DeductionAmount = result.employerAmount,
                        BenefitType = employeeBenefit.BenefitType ?? "Unknown",
                        Percentage = employeeBenefit.BenefitType == "Porcentual" ? employeeBenefit.BenefitValue : null
                    });
                }

                // Acumular deducciones del empleado (NO guardar todav�a)
                if (result.employeeDeductions.Any())
                {
                    employeeDeductions.AddRange(result.employeeDeductions);
                }
            }

            Console.WriteLine("PASO TODOS LOS CALCULOS");

            // Guardar todas las deducciones del empleado
            if (employeeDeductions.Any())
            {
                _employeeDeductionService.SaveEmployeeDeductions(employeeDeductions);
            }

            Console.WriteLine("SE GUARDO DEDUCUOIN DE EMPLEADO: " + employeeDeductions);

            // Guardar todas las deducciones del empleador
            if (employerDeductions.Any())
            {
                _employerBenefitDeductionService.SaveEmployerBenefitDeductions(employerDeductions);
            }
            Console.WriteLine("SE GUARDO DEDUCUOIN DE EMPLEADOR");

            return Math.Round(totalEmployerCost, 2);
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
                    
                    // Separar deducciones de empleador y empleado
                    foreach (var deduction in apiResult.Deductions ?? new List<ApiDeduction>())
                    {
                        if (deduction.Type == "ER") // Employer
                        {
                            employerAmount += deduction.Amount;
                        }
                        else if (deduction.Type == "EE") // Employee
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
                var normalizedApiName = apiName?.ToLowerInvariant() ?? string.Empty;

                switch (normalizedApiName)
                {
                    case "asociacion solidarista":
                    case "asociacionsolidarista":
                        var solidarityService = _apiFactory.CreateSolidarityAssociationService();
                        return await solidarityService.CalculateContributionAsync(new SolidarityAssociationRequest
                        {
                            CompanyLegalId = idCompany.ToString(),
                            GrossSalary = employee.SalarioBruto
                        });

                    case "privateinsurance":
                    case "seguroprivado":
                    case "seguro privado":
                        var insuranceService = _apiFactory.CreatePrivateInsuranceService();
                        return await insuranceService.CalculatePremiumAsync(new PrivateInsuranceRequest
                        {
                            BirthDate = employee.Cumpleanos,
                            Age = 0,
                            DependentsCount = dependentsCount ?? 0
                        });

                    case "voluntarypensions":
                    case "pensionesvoluntarias":
                    case "pensiones voluntarias":
                        var pensionsService = _apiFactory.CreateVoluntaryPensionsService();
                        return await pensionsService.CalculatePremiumAsync(new VoluntaryPensionsRequest
                        {
                            PlanType = pensionType.ToUpper() ?? "A",
                            GrossSalary = employee.SalarioBruto
                        });

                    default:
                        Console.WriteLine($"API no reconocida: {apiName}");
                        return new ExternalApiResponse();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling external API {apiName}: {ex.Message}");
                return new ExternalApiResponse();
            }
        }

        public List<BenefitDto> GetBenefitsList(long cedulaJuridicaEmpresa)
        {
            // DEPRECATED: Este m�todo ya no se usa
            return new List<BenefitDto>();
        }
    }
}