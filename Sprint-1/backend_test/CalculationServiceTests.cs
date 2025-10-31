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
            var mockEmpDed = new Mock<ICalculatorDeductionsEmployeeService>();
            var mockTax = new Mock<IPersonalIncomeTaxService>();
            var mockBenefits = new Mock<ICalculatorBenefitsService>();
            var mockLogger = new Mock<ILogger<CalculationService>>();

            decimal salary = 100000m;
            decimal expectedTax = 1234.56m;
            mockTax.Setup(t => t.CalculateIncomeTax(salary)).Returns(expectedTax);

            var svc = new CalculationService(
                mockDedEmp.Object,
                mockEmpDed.Object,
                mockTax.Object,
                mockBenefits.Object,
                mockLogger.Object);

            var dto = new EmployeeCalculationDto { SalarioBruto = salary, CedulaEmpleado = 1, NombreEmpleado = "Test" };

            // Act
            var tax = await svc.CalculateIncomeTaxAsync(dto, companyId: 1, payrollId: 1);

            // Assert
            Assert.AreEqual(expectedTax, tax);
        }
    }
}
