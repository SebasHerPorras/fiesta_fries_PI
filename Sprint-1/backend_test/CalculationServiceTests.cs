using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using backend.Interfaces;
using backend.Interfaces.Services;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;

namespace backend_test
{
    [TestFixture]
    public class CalculationServiceTests
    {
        [Test]
        public async Task CalculateIncomeTax_OnBoundary_ReturnsExpected()
        {
            // Arrange
            var mockDedEmp = new Mock<ICalculatorDeductionsEmployerService>();
            var mockTax = new Mock<IPersonalIncomeTaxService>();
            var mockLogger = new Mock<ILogger<CalculationService>>();
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockSocialSecurity = new Mock<IEmployeeSocialSecurityContributionsService>();
            var mockPayrollDeduction = new Mock<IEmployeeDeductionsByPayrollService>();
            var mockEmployerBenefit = new Mock<IEmployerBenefitDeductionService>();
            var mockApiFactory = new Mock<IExternalApiFactory>();
            var mockEmployeeBenefit = new Mock<IEmployeeBenefitService>();
            var mockEmployerSocialSecurity = new Mock<IEmployerSocialSecurityContributionsService>();
            var mockEmployerPayroll = new Mock<IEmployerSocialSecurityByPayrollService>();

            decimal salary = 100000m;
            decimal expectedTax = 1234.56m;
            mockTax.Setup(t => t.CalculateIncomeTax(salary)).Returns(expectedTax);

            var svc = new CalculationService(
                mockDedEmp.Object,
                mockTax.Object,
                mockLogger.Object,
                mockServiceProvider.Object,
                mockSocialSecurity.Object,
                mockPayrollDeduction.Object,
                mockEmployerBenefit.Object,
                mockApiFactory.Object,
                mockEmployeeBenefit.Object,
                mockEmployerSocialSecurity.Object,
                mockEmployerPayroll.Object
            );

            var dto = new EmployeeCalculationDto { SalarioBruto = salary, CedulaEmpleado = 1, NombreEmpleado = "Test" };

            // Act
            var tax = await svc.CalculateIncomeTaxAsync(dto, companyId: 1, payrollId: 1);

            // Assert
            Assert.AreEqual(expectedTax, tax);
        }
    }
}
