using backend.Models;
using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalApisTestController : ControllerBase
    {
        private readonly IExternalApiFactory _apiFactory;

        public ExternalApisTestController(IExternalApiFactory apiFactory)
        {
            _apiFactory = apiFactory;
        }

        [HttpGet("test-all")]
        public async Task<ActionResult> TestAll(
            [FromQuery] string companyLegalId = "5",
            [FromQuery] decimal solidarityGrossSalary = 2000000,
            [FromQuery] string birthDate = "2004-04-22",
            [FromQuery] int dependentsCount = 2,
            [FromQuery] string pensionPlanType = "A",
            [FromQuery] decimal pensionGrossSalary = 1000000)
        {
            // Parsear la fecha de nacimiento
            DateTime employeeBirthDate;
            if (!DateTime.TryParse(birthDate, out employeeBirthDate))
            {
                employeeBirthDate = new DateTime(2004, 4, 22); // Fecha por defecto: 22 de abril de 2004
            }

            var results = new
            {
                timestamp = DateTime.UtcNow,
                tests = new
                {
                    solidarityAssociation = new
                    {
                        service = "Solidarity Association",
                        request = new { companyLegalId, grossSalary = solidarityGrossSalary },
                        response = await TestSolidarityAssociation(companyLegalId, solidarityGrossSalary)
                    },
                    privateInsurance = new
                    {
                        service = "Private Insurance",
                        request = new { birthDate = employeeBirthDate, dependentsCount },
                        response = await TestPrivateInsurance(employeeBirthDate, dependentsCount)
                    },
                    voluntaryPensions = new
                    {
                        service = "Voluntary Pensions",
                        request = new { planType = pensionPlanType, grossSalary = pensionGrossSalary },
                        response = await TestVoluntaryPensions(pensionPlanType, pensionGrossSalary)
                    }
                }
            };

            return Ok(results);
        }

        private async Task<ExternalApiResponse> TestSolidarityAssociation(string companyLegalId, decimal grossSalary)
        {
            try
            {
                var service = _apiFactory.CreateSolidarityAssociationService();
                return await service.CalculateContributionAsync(new SolidarityAssociationRequest
                {
                    CompanyLegalId = companyLegalId,
                    GrossSalary = grossSalary
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing Solidarity Association: {ex.Message}");
                return new ExternalApiResponse();
            }
        }

        private async Task<ExternalApiResponse> TestPrivateInsurance(DateTime birthDate, int dependentsCount)
        {
            try
            {
                var service = _apiFactory.CreatePrivateInsuranceService();
                return await service.CalculatePremiumAsync(new PrivateInsuranceRequest
                {
                    BirthDate = birthDate,
                    Age = 0, // El service calculará la edad
                    DependentsCount = dependentsCount
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing Private Insurance: {ex.Message}");
                return new ExternalApiResponse();
            }
        }

        private async Task<ExternalApiResponse> TestVoluntaryPensions(string planType, decimal grossSalary)
        {
            try
            {
                var service = _apiFactory.CreateVoluntaryPensionsService();
                return await service.CalculatePremiumAsync(new VoluntaryPensionsRequest
                {
                    PlanType = planType,
                    GrossSalary = grossSalary
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error testing Voluntary Pensions: {ex.Message}");
                return new ExternalApiResponse();
            }
        }
    }
}