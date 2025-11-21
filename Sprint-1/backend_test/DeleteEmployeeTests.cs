using System;
using System.Threading.Tasks;
using backend.Interfaces;
using backend.Models;
using backend.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;


namespace Tests.Services
{
    [TestFixture]
    public class EmployeeDeletionServiceTests
    {
        private Mock<IEmployeeDeletionRepository> _repoMock;
        private Mock<ILogger<EmployeeDeletionService>> _loggerMock;
        private EmployeeDeletionService _service;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IEmployeeDeletionRepository>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<EmployeeDeletionService>>();
            _service = new EmployeeDeletionService(_repoMock.Object, _loggerMock.Object);
        }

        [Test]
        public async Task DeleteEmpleado_AplicaPhysical_WhenNoPayrollPayments()
        {
            // Arrange
            int personaId = 1001;
            long companyId = 55555555;

            _repoMock.Setup(r => r.ValidateEmployeeExistsAsync(personaId, companyId))
                     .ReturnsAsync(true);
            _repoMock.Setup(r => r.IsCompanyOwnerAsync(personaId))
                     .ReturnsAsync(false);

            var payrollStatus = new EmployeePayrollStatus { HasPayments = false, PaymentCount = 0 };
            _repoMock.Setup(r => r.CheckPayrollStatusAsync(personaId))
                     .ReturnsAsync(payrollStatus);

            _repoMock.Setup(r => r.ExecutePhysicalDeleteAsync(personaId))
                     .ReturnsAsync(true)
                     .Verifiable();

            // Act
            var result = await _service.DeleteEmpleadoAsync(personaId, companyId);

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(DeletionType.Physical, result.DeletionType);
            Assert.IsFalse(result.PayrollStatus.HasPayments);
            _repoMock.Verify(r => r.ValidateEmployeeExistsAsync(personaId, companyId), Times.Once);
            _repoMock.Verify(r => r.ExecutePhysicalDeleteAsync(personaId), Times.Once);
        }

        [Test]
        public async Task DeleteEmpleado_AplicaLogical_WhenHasPayrollPayments()
        {
            // Arrange
            int personaId = 2002;
            long companyId = 66666666;

            _repoMock.Setup(r => r.ValidateEmployeeExistsAsync(personaId, companyId))
                     .ReturnsAsync(true);
            _repoMock.Setup(r => r.IsCompanyOwnerAsync(personaId))
                     .ReturnsAsync(false);

            var payrollStatus = new EmployeePayrollStatus { HasPayments = true, PaymentCount = 3 };
            _repoMock.Setup(r => r.CheckPayrollStatusAsync(personaId))
                     .ReturnsAsync(payrollStatus);

            _repoMock.Setup(r => r.ExecuteLogicalDeleteAsync(personaId))
                     .ReturnsAsync(true)
                     .Verifiable();

            // Act
            var result = await _service.DeleteEmpleadoAsync(personaId, companyId);

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(DeletionType.Logical, result.DeletionType);
            Assert.IsTrue(result.PayrollStatus.HasPayments);
            Assert.AreEqual(3, result.PayrollStatus.PaymentCount);
            _repoMock.Verify(r => r.ExecuteLogicalDeleteAsync(personaId), Times.Once);
        }

        [Test]
        public async Task DeleteEmpleado_ReturnsError_WhenRepositoryThrows()
        {
            // Arrange
            int personaId = 3003;
            long companyId = 77777777;

            _repoMock.Setup(r => r.ValidateEmployeeExistsAsync(personaId, companyId))
                     .ThrowsAsync(new Exception("DB unavailable"));

            // Act
            var result = await _service.DeleteEmpleadoAsync(personaId, companyId);

            // Assert
            Assert.NotNull(result);
            Assert.IsFalse(result.Success);
            StringAssert.Contains("Error", result.Message);
            _repoMock.Verify(r => r.ValidateEmployeeExistsAsync(personaId, companyId), Times.Once);
        }
    }
}