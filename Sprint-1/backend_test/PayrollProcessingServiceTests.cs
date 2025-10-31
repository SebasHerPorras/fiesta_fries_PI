using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Services;
using backend.Interfaces.Services;
using backend.Interfaces;
using backend.Models;
using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using backend.Models.Payroll;
using Microsoft.Extensions.Logging;

namespace backend_test
{
    [TestFixture]
    public class PayrollProcessingServiceTests
    {
        [Test]
        public async Task PreviewPayrollAsync_DoesNotPersist()
        {
            // Arrange
            var mockPayrollRepo = new Mock<IPayrollRepository>();
            var mockCalc = new Mock<ICalculationService>();
            var mockEmployeeService = new Mock<IEmployeeService>();
            var mockValidator = new Mock<IPayrollValidator>();
            var mockResultBuilder = new Mock<IPayrollResultBuilder>();
            var mockPeriodService = new Mock<IPayrollPeriodService>();
            var mockLogger = new Mock<ILogger<PayrollProcessingService>>();

            var dto = new EmployeeCalculationDto { CedulaEmpleado = 12345, NombreEmpleado = "Juan", SalarioBruto = 1000m, HorasTrabajadas = 40 };
            mockEmployeeService.Setup(s => s.GetEmployeeCalculationDtos(It.IsAny<long>(), It.IsAny<System.DateTime>(), It.IsAny<System.DateTime>()))
                .Returns(new List<EmployeeCalculationDto> { dto });

            mockCalc.Setup(c => c.CalculateDeductionsAsync(It.IsAny<EmployeeCalculationDto>(), It.IsAny<long>(), It.IsAny<int>())).ReturnsAsync(100m);
            mockCalc.Setup(c => c.CalculateBenefitsAsync(It.IsAny<EmployeeCalculationDto>(), It.IsAny<long>(), It.IsAny<int>())).ReturnsAsync(50m);
            mockCalc.Setup(c => c.CalculateEmployerDeductionsAsync(It.IsAny<EmployeeCalculationDto>(), It.IsAny<long>(), It.IsAny<int>())).ReturnsAsync(10m);

            var previewResult = new PayrollProcessResult { Success = true, ProcessedEmployees = 1, TotalAmount = 960m };
            mockResultBuilder.Setup(r => r.CreatePreviewResult(It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                .Returns(previewResult);

            var sut = new PayrollProcessingService(
                mockPayrollRepo.Object,
                mockCalc.Object,
                mockEmployeeService.Object,
                mockValidator.Object,
                mockResultBuilder.Object,
                mockPeriodService.Object,
                mockLogger.Object
            );

            var request = new backend.Controllers.PayrollController.PayrollPreviewRequest { CompanyId = 1, PeriodDate = System.DateTime.Now.Date };

            // Act
            var result = await sut.PreviewPayrollAsync(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);

            mockPayrollRepo.Verify(r => r.CreatePayrollAsync(It.IsAny<Payroll>()), Times.Never);
            mockPayrollRepo.Verify(r => r.UpdatePayrollAsync(It.IsAny<Payroll>()), Times.Never);
            mockPayrollRepo.Verify(r => r.CreatePayrollPaymentsAsync(It.IsAny<List<PayrollPayment>>()), Times.Never);
        }
    }
}
