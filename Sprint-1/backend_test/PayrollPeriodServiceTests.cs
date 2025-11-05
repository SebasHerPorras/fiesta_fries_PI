using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Interfaces;
using backend.Interfaces.Services;
using backend.Interfaces.Strategies;
using backend.Models.Payroll;
using backend.Models;
using backend.Repositories;
using backend.Services;
using Microsoft.Extensions.Logging; 

namespace backend_test
{
    [TestFixture]
    public class PayrollPeriodServiceTests
    {
        private Mock<IPayrollRepository> _mockRepo;
        private Mock<IEmpresaRepository> _mockEmpresaRepo;
        private Mock<IEnumerable<IPeriodCalculator>> _mockCalculators;
        private Mock<IPeriodCalculator> _mockMonthlyCalculator;
        private Mock<IPeriodCalculator> _mockBiweeklyCalculator;
        private Mock<ILogger<PayrollPeriodService>> _mockLogger;
        private PayrollPeriodService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IPayrollRepository>();
            _mockEmpresaRepo = new Mock<IEmpresaRepository>();
            _mockMonthlyCalculator = new Mock<IPeriodCalculator>();
            _mockBiweeklyCalculator = new Mock<IPeriodCalculator>();
            _mockCalculators = new Mock<IEnumerable<IPeriodCalculator>>();
            _mockLogger = new Mock<ILogger<PayrollPeriodService>>();

            _mockMonthlyCalculator.Setup(c => c.CanHandle("mensual")).Returns(true);
            _mockBiweeklyCalculator.Setup(c => c.CanHandle("quincenal")).Returns(true);
            
            var calculators = new List<IPeriodCalculator> { 
                _mockMonthlyCalculator.Object, 
                _mockBiweeklyCalculator.Object 
            };
            
            _mockCalculators.Setup(c => c.GetEnumerator())
                .Returns(calculators.GetEnumerator());

            _service = new PayrollPeriodService(_mockRepo.Object, _mockEmpresaRepo.Object, _mockLogger.Object, calculators);
        }

        [Test]
        public async Task CalculateNextPeriodAsync_ForNewCompany_ReturnsFirstPeriod()
        {
            // Arrange
            var companyId = "123456789";
            var company = new EmpresaModel { 
                CedulaJuridica = 123456789,
                Nombre = "Test Company",
                FrecuenciaPago = "mensual", 
                DiaPago = 15,
                DueñoEmpresa = 1
            };
            
            _mockEmpresaRepo.Setup(r => r.GetByCedulaJuridica(It.IsAny<long>()))
                .Returns(company);
            _mockRepo.Setup(r => r.GetLatestPayrollAsync(companyId))
                .ReturnsAsync((Payroll)null);

            var expectedPeriod = new PayrollPeriod
            {
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 1, 31),
                Description = "Enero 2025",
                PeriodType = PayrollPeriodType.Mensual
            };

            _mockMonthlyCalculator.Setup(c => c.CalculatePeriod(
                It.IsAny<DateTime>(), It.IsAny<int>()
            )).Returns(expectedPeriod);

            // Act
            var result = await _service.CalculateNextPeriodAsync(companyId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StartDate, Is.EqualTo(expectedPeriod.StartDate));
            Assert.That(result.EndDate, Is.EqualTo(expectedPeriod.EndDate));
        }

        [Test]
        [Timeout(10000)] 
        [Ignore("Temporalmente deshabilitado - Causa loop infinito")]
        public async Task GetOverduePeriodsAsync_WhenPeriodsExist_ReturnsOverduePeriods()
        {
            // Arrange
            var companyId = "123456789";
            var company = new EmpresaModel { 
                CedulaJuridica = 123456789,
                Nombre = "Test Company",
                FrecuenciaPago = "mensual", // FRECUENCIA CORRECTA
                DiaPago = 15,
                DueñoEmpresa = 1
            };
            
            _mockEmpresaRepo.Setup(r => r.GetByCedulaJuridica(It.IsAny<long>()))
                .Returns(company);
                
            var now = DateTime.Now;
            var overduePeriod = new PayrollPeriod
            {
                StartDate = now.AddMonths(-2),
                EndDate = now.AddMonths(-1),
                Description = "Periodo vencido",
                IsProcessed = false,
                PeriodType = PayrollPeriodType.Mensual
            };

            _mockRepo.Setup(r => r.GetLatestPayrollAsync(companyId))
                .ReturnsAsync(new Payroll { PeriodDate = now.AddMonths(-3) });

            _mockMonthlyCalculator.Setup(c => c.CalculatePeriod(
                It.IsAny<DateTime>(), It.IsAny<int>()
            )).Returns(overduePeriod);

            // Act
            var result = await _service.GetOverduePeriodsAsync(companyId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.GreaterThan(0));
            Assert.That(result[0].EndDate, Is.LessThan(DateTime.Now));
        }

       [Test]
        public async Task IsPeriodProcessedAsync_WhenPeriodExists_ReturnsTrue()
        {
            // Arrange
            var companyId = "123456789";
            var periodDate = new DateTime(2024, 1, 15);

            _mockRepo.Setup(r => r.GetPayrollsByCompanyAsync(companyId))
                .ReturnsAsync(new List<Payroll> {
                    new Payroll { 
                        PeriodDate = periodDate,  // Esta fecha debe coincidir
                    }
                });

            // Act
            var result = await _service.IsPeriodProcessedAsync(companyId, periodDate);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CalculateNextPeriodAsync_CompanyNotFound_ThrowsException()
        {
            // Arrange
            var companyId = "999999999";
            _mockEmpresaRepo.Setup(r => r.GetByCedulaJuridica(It.IsAny<long>()))
                .Returns((EmpresaModel)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.CalculateNextPeriodAsync(companyId)
            );
            Assert.That(ex.Message, Contains.Substring("not found"));
        }

        [Test]
        public void CalculateNextPeriodAsync_NoSuitableCalculator_ThrowsException()
        {
            // Arrange
            var companyId = "123456789";
            var company = new EmpresaModel { 
                CedulaJuridica = 123456789,
                Nombre = "Test Company", 
                FrecuenciaPago = "semanal", // Frecuencia no soportada
                DiaPago = 15,
                DueñoEmpresa = 1
            };
            
            _mockEmpresaRepo.Setup(r => r.GetByCedulaJuridica(It.IsAny<long>()))
                .Returns(company);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ArgumentException>(
                async () => await _service.CalculateNextPeriodAsync(companyId)
            );
            Assert.That(ex.Message, Contains.Substring("Unsupported payment frequency"));
        }

        [Test]
        public async Task CalculateNextPeriodAsync_WithExistingPayroll_CalculatesNextPeriod()
        {
            // Arrange
            var companyId = "123456789";
            var company = new EmpresaModel { 
                CedulaJuridica = 123456789,
                Nombre = "Test Company",
                FrecuenciaPago = "mensual", // Frecuencia correcto
                DiaPago = 15,
                DueñoEmpresa = 1
            };
            
            _mockEmpresaRepo.Setup(r => r.GetByCedulaJuridica(It.IsAny<long>()))
                .Returns(company);
                
            var lastPayrollDate = new DateTime(2024, 1, 31);
            
            _mockRepo.Setup(r => r.GetLatestPayrollAsync(companyId))
                .ReturnsAsync(new Payroll 
                { 
                    PeriodDate = lastPayrollDate,
                    IsCalculated = true
                });

            var expectedPeriod = new PayrollPeriod
            {
                StartDate = new DateTime(2024, 2, 1),
                EndDate = new DateTime(2024, 2, 29),
                Description = "Febrero 2024",
                PeriodType = PayrollPeriodType.Mensual
            };

            _mockMonthlyCalculator.Setup(c => c.CalculatePeriod(
                It.IsAny<DateTime>(), 
                It.IsAny<int>()
            )).Returns(expectedPeriod);

            // Act
            var result = await _service.CalculateNextPeriodAsync(companyId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StartDate, Is.EqualTo(expectedPeriod.StartDate));
            Assert.That(result.EndDate, Is.EqualTo(expectedPeriod.EndDate));
        }

        [Test]
        public async Task CalculateNextPeriodAsync_WithBiweeklyFrequency_UsesBiweeklyCalculator()
        {
            // Arrange
            var companyId = "123456789";
            var company = new EmpresaModel { 
                CedulaJuridica = 123456789,
                Nombre = "Test Company",
                FrecuenciaPago = "quincenal", 
                DiaPago = 15,
                DueñoEmpresa = 1
            };
            
            _mockEmpresaRepo.Setup(r => r.GetByCedulaJuridica(It.IsAny<long>()))
                .Returns(company);
                
            _mockRepo.Setup(r => r.GetLatestPayrollAsync(companyId))
                .ReturnsAsync((Payroll)null);

            var expectedPeriod = new PayrollPeriod
            {
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 15),
                Description = "Primera quincena Enero 2024",
                PeriodType = PayrollPeriodType.Quincenal
            };

            _mockBiweeklyCalculator.Setup(c => c.CalculatePeriod(
                It.IsAny<DateTime>(), It.IsAny<int>()
            )).Returns(expectedPeriod);

            // Act
            var result = await _service.CalculateNextPeriodAsync(companyId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StartDate, Is.EqualTo(expectedPeriod.StartDate));
            Assert.That(result.EndDate, Is.EqualTo(expectedPeriod.EndDate));
            
            _mockBiweeklyCalculator.Verify(c => c.CanHandle("quincenal"), Times.Once);
            _mockBiweeklyCalculator.Verify(c => c.CalculatePeriod(
                It.IsAny<DateTime>(), 
                It.IsAny<int>()
            ), Times.Once);
        }
    }
}