using backend.Models;
using backend.Interfaces;

namespace backend.Services
{
    public class CalculatorBenefitsService : ICalculatorBenefitsService
    {
        private readonly IEmployeeDeductionsByPayrollService _employeeDeductionService;
        private readonly IEmployerBenefitDeductionService _employerBenefitDeductionService;
        private readonly IExternalApiFactory _apiFactory;

        public CalculatorBenefitsService(
            IEmployeeDeductionsByPayrollService employeeDeductionService,
            IEmployerBenefitDeductionService employerBenefitDeductionService,
            IExternalApiFactory apiFactory)
        {
            _employeeDeductionService = employeeDeductionService;
            _employerBenefitDeductionService = employerBenefitDeductionService;
            _apiFactory = apiFactory;
        }

        public async Task<decimal> CalculateBenefitsAsync(EmployeeCalculationDto employee, int reportId, long cedulaJuridicaEmpresa)
        {
            if (employee == null)
                throw new ArgumentException("Los datos del empleado son requeridos");

            if (employee.SalarioBruto <= 0)
                throw new ArgumentException("El salario bruto debe ser mayor a cero");

            // Obtener lista de beneficios (hardcoded por ahora)
            var benefits = GetBenefitsList(cedulaJuridicaEmpresa);

            decimal totalEmployerCost = 0;
            var employerDeductions = new List<EmployerBenefitDeductionDto>();

            foreach (var benefit in benefits)
            {
                var result = await ProcessBenefitAsync(benefit, employee, reportId, cedulaJuridicaEmpresa);
                
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
                        BenefitName = benefit.Name,
                        DeductionAmount = result.employerAmount,
                        BenefitType = benefit.Type,
                        Percentage = benefit.Type == "Porcentual" ? benefit.Value : null
                    });
                }

                // Si hay deducción para el empleado (solo en caso de API), guardar
                if (result.employeeDeductions.Any())
                {
                    _employeeDeductionService.SaveEmployeeDeductions(result.employeeDeductions);
                }
            }

            // Guardar todas las deducciones del empleador
            if (employerDeductions.Any())
            {
                _employerBenefitDeductionService.SaveEmployerBenefitDeductions(employerDeductions);
            }

            return Math.Round(totalEmployerCost, 2);
        }

        private async Task<(decimal employerAmount, List<EmployeeDeductionsByPayrollDto> employeeDeductions)> 
            ProcessBenefitAsync(BenefitDto benefit, EmployeeCalculationDto employee, int reportId, long cedulaJuridicaEmpresa)
        {
            decimal employerAmount = 0;
            var employeeDeductions = new List<EmployeeDeductionsByPayrollDto>();

            switch (benefit.Type.ToUpperInvariant())
            {
                case "MONTO FIJO":
                    employerAmount = benefit.Value;
                    break;

                case "PORCENTUAL":
                    employerAmount = Math.Round(employee.SalarioBruto * (benefit.Value / 100), 2);
                    break;

                case "API":
                    var apiResult = await CallExternalApiAsync(benefit.ApiName, employee);
                    
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
                                DeductionName = $"Beneficio: {benefit.Name} (Empleado)",
                                DeductionAmount = deduction.Amount,
                                Percentage = null
                            });
                        }
                    }
                    break;

                default:
                    Console.WriteLine($"Tipo de beneficio no reconocido: {benefit.Type}");
                    break;
            }

            return (employerAmount, employeeDeductions);
        }

        private async Task<ExternalApiResponse> CallExternalApiAsync(string apiName, EmployeeCalculationDto employee)
        {
            try
            {
                switch (apiName.ToLowerInvariant())
                {
                    case "solidarityassociation":
                    case "asociacion":
                        var solidarityService = _apiFactory.CreateSolidarityAssociationService();
                        return await solidarityService.CalculateContributionAsync(new SolidarityAssociationRequest
                        {
                            CompanyLegalId = employee.CedulaEmpleado.ToString(),
                            GrossSalary = employee.SalarioBruto
                        });

                    case "privateinsurance":
                    case "seguro":
                        var insuranceService = _apiFactory.CreatePrivateInsuranceService();
                        return await insuranceService.CalculatePremiumAsync(new PrivateInsuranceRequest
                        {
                            BirthDate = employee.Cumpleanos,
                            Age = 0,
                            DependentsCount = 1 // TODO: Obtener de algún lugar
                        });

                    case "voluntarypensions":
                    case "pensiones":
                        var pensionsService = _apiFactory.CreateVoluntaryPensionsService();
                        return await pensionsService.CalculatePremiumAsync(new VoluntaryPensionsRequest
                        {
                            PlanType = "A", // TODO: Obtener de algún lugar
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
            // HARDCODED - Lista quemada por ahora
            // TODO: Reemplazar con llamada a repositorio cuando esté listo

            if (cedulaJuridicaEmpresa == 3101123456) // Fiesta Fries
            {
                return new List<BenefitDto>
                {
                    new BenefitDto { Id = 1, Name = "Seguro Médico", Type = "Porcentual", Value = 8.00m },
                    new BenefitDto { Id = 2, Name = "Transporte", Type = "Monto Fijo", Value = 15000m },
                    new BenefitDto { Id = 3, Name = "Almuerzo", Type = "Monto Fijo", Value = 5000m },
                    new BenefitDto { Id = 4, Name = "Asociación Solidarista", Type = "API", Value = 0, ApiName = "SolidarityAssociation" }
                };
            }
            else if (cedulaJuridicaEmpresa == 3102234567) // Taco Bell
            {
                return new List<BenefitDto>
                {
                    new BenefitDto { Id = 5, Name = "Seguro de Vida", Type = "Porcentual", Value = 12.00m },
                    new BenefitDto { Id = 6, Name = "Bonificación", Type = "Monto Fijo", Value = 20000m },
                    new BenefitDto { Id = 7, Name = "Seguro Privado", Type = "API", Value = 0, ApiName = "PrivateInsurance" },
                    new BenefitDto { Id = 8, Name = "Pensiones Voluntarias", Type = "API", Value = 0, ApiName = "VoluntaryPensions" }
                };
            }

            return new List<BenefitDto>();
        }
    }
}