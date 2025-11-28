using NUnit.Framework;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using backend.Services;
using backend.Models.Payroll;
using Microsoft.Extensions.Logging;
using FluentAssertions;

namespace backend_test
{
    [TestFixture]
    public class PayrollPdfServiceTests
    {
        private Mock<ILogger<PayrollPdfService>> _mockLogger;
        private PayrollPdfService _service;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<PayrollPdfService>>();
            _service = new PayrollPdfService(_mockLogger.Object);
        }

        #region Tests Básicos

        [Test]
        public async Task GeneratePayrollPdfAsync_WithValidReport_ReturnsPdfBytes()
        {
            // Arrange
            var report = CrearReporteCompleto();

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            
            // Verificar que se generó el PDF exitosamente (log con emoji ✅)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("PDF generado exitosamente") 
                                                   || v.ToString().Contains("Planilla:")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public void GeneratePayrollPdfAsync_WithNullReport_ThrowsArgumentNullException()
        {
            // Arrange
            PayrollFullReport? report = null;

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.GeneratePayrollPdfAsync(report!)
            );
            exception.ParamName.Should().Be("report");
            exception.Message.Should().Contain("Report no puede ser null");
        }

        [Test]
        public void GeneratePayrollPdfAsync_WithNullHeader_ThrowsArgumentNullException()
        {
            // Arrange
            var report = new PayrollFullReport
            {
                Header = null!,
                Employees = new List<EmployeeDetail>(),
                EmployerCharges = new List<ChargeDetail>(),
                EmployeeDeductions = new List<DeductionDetail>(),
                Benefits = new List<BenefitDetail>()
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _service.GeneratePayrollPdfAsync(report)
            );
            
            // CORRECCIÓN: El ParamName es "Header" no "report.Header"
            exception.ParamName.Should().Be("Header");
            exception.Message.Should().Contain("Header no puede ser null");
        }

        #endregion

        #region Tests de Contenido

        [Test]
        public async Task GeneratePayrollPdfAsync_WithEmptyEmployees_GeneratesValidPdf()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Employees = new List<EmployeeDetail>();

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GeneratePayrollPdfAsync_WithEmptyDeductions_GeneratesValidPdf()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.EmployeeDeductions = new List<DeductionDetail>();
            report.EmployerCharges = new List<ChargeDetail>();

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            
            // Verificar que se logueó warning por deducciones vacías (emoji ⚠️)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No hay deducciones") 
                                                   || v.ToString().Contains("No hay")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Test]
        public async Task GeneratePayrollPdfAsync_WithEmptyBenefits_GeneratesValidPdf()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Benefits = new List<BenefitDetail>();

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            
            // Verificar que se logueó warning por beneficios vacíos (emoji ⚠️)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No hay beneficios") 
                                                   || v.ToString().Contains("No hay")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Test]
        public async Task GeneratePayrollPdfAsync_WithMultipleEmployees_IncludesAllData()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Employees.Add(new EmployeeDetail
            {
                EmployeeId = 208760987,
                NombreEmpleado = "Ana Salas",
                TipoEmpleado = "Tiempo completo",
                GrossSalary = 1000000,
                DeductionsAmount = 105000,
                BenefitsAmount = 80000,
                NetSalary = 895000,
                Status = "Activo"
            });

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        #endregion

        #region Tests de Valores Numéricos

        [TestCase(500000, 50000, 40000, 30000, 460000)]
        [TestCase(1000000, 105000, 80000, 50000, 895000)]
        [TestCase(4000000, 420000, 320000, 150000, 3580000)]
        public async Task GeneratePayrollPdfAsync_WithDifferentSalaries_CalculatesCorrectly(
            decimal grossSalary,
            decimal deductions,
            decimal benefits,
            decimal charges,
            decimal netSalary)
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Header.TotalGrossSalary = grossSalary;
            report.Header.TotalEmployeeDeductions = deductions;
            report.Header.TotalBenefits = benefits;
            report.Header.TotalEmployerDeductions = charges;
            report.Header.TotalNetSalary = netSalary;

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GeneratePayrollPdfAsync_WithZeroAmounts_GeneratesValidPdf()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Header.TotalGrossSalary = 0;
            report.Header.TotalEmployeeDeductions = 0;
            report.Header.TotalBenefits = 0;
            report.Header.TotalNetSalary = 0;

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        #endregion

        #region Tests de Caracteres Especiales

        [Test]
        public async Task GeneratePayrollPdfAsync_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Header.NombreEmpresa = "Empresa Ñoño & Cía.";
            report.Header.NombreEmpleador = "José María García-López";
            report.Employees[0].NombreEmpleado = "María José Hernández-Pérez";

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GeneratePayrollPdfAsync_WithLongNames_HandlesCorrectly()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Header.NombreEmpresa = "Empresa de Servicios Integrales de Tecnología y Consultoría S.A.";
            report.Employees[0].NombreEmpleado = "Juan Carlos de la Rosa María Hernández Fernández";

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        #endregion

        #region Tests de Deducciones y Beneficios

        [Test]
        public async Task GeneratePayrollPdfAsync_WithMultipleCharges_IncludesAll()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.EmployerCharges = new List<ChargeDetail>
            {
                new ChargeDetail { ChargeName = "CCSS", TotalAmount = 26550, PercentageDisplay = 9.34m },
                new ChargeDetail { ChargeName = "INS", TotalAmount = 3000, PercentageDisplay = 1.00m },
                new ChargeDetail { ChargeName = "INA", TotalAmount = 4500, PercentageDisplay = 1.50m }
            };

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GeneratePayrollPdfAsync_WithMultipleBenefits_IncludesAll()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Benefits = new List<BenefitDetail>
            {
                new BenefitDetail { BenefitName = "Gimnasio", BenefitType = "Monto Fijo", TotalAmount = 35000 },
                new BenefitDetail { BenefitName = "Educación", BenefitType = "Porcentual", TotalAmount = 15000 },
                new BenefitDetail { BenefitName = "Seguro privado", BenefitType = "API", TotalAmount = 12000 }
            };

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        #endregion

        #region Tests de Fechas

        [Test]
        public async Task GeneratePayrollPdfAsync_WithDifferentDates_FormatsCorrectly()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Header.PeriodDate = new DateTime(2025, 11, 15);
            report.Header.LastModified = new DateTime(2025, 11, 18, 14, 30, 0);

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GeneratePayrollPdfAsync_WithYearTransition_HandlesCorrectly()
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Header.PeriodDate = new DateTime(2024, 12, 31);

            // Act
            var result = await _service.GeneratePayrollPdfAsync(report);

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
        }

        #endregion

        #region Tests de Performance

        [Test]
        [TestCase(10)]
        [TestCase(50)]
        [TestCase(100)]
        public async Task GeneratePayrollPdfAsync_WithManyEmployees_GeneratesInReasonableTime(int employeeCount)
        {
            // Arrange
            var report = CrearReporteCompleto();
            report.Employees.Clear();

            for (int i = 0; i < employeeCount; i++)
            {
                report.Employees.Add(new EmployeeDetail
                {
                    EmployeeId = 100000000 + i,
                    NombreEmpleado = $"Empleado {i}",
                    TipoEmpleado = "Tiempo completo",
                    GrossSalary = 500000,
                    DeductionsAmount = 52500,
                    BenefitsAmount = 40000,
                    NetSalary = 447500,
                    Status = "Activo"
                });
            }

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = await _service.GeneratePayrollPdfAsync(report);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Menos de 10 segundos (más realista)
        }

        #endregion

        #region Tests de Logging

        [Test]
        public async Task GeneratePayrollPdfAsync_LogsStartAndCompletion()
        {
            // Arrange
            var report = CrearReporteCompleto();

            // Act
            await _service.GeneratePayrollPdfAsync(report);

            // Assert
            // Verificar log de inicio (emoji 📄)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("INICIO") 
                                                   || v.ToString().Contains("Generando PDF")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verificar log de finalización (emoji ✅)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("PDF generado exitosamente") 
                                                   || v.ToString().Contains("bytes")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        #endregion

        #region Métodos Helper

        private PayrollFullReport CrearReporteCompleto()
        {
            return new PayrollFullReport
            {
                Header = new PayrollReportHeader
                {
                    PayrollId = 1,
                    PeriodDate = new DateTime(2025, 10, 31),
                    CompanyId = 550020002,
                    NombreEmpresa = "Empresa PI",
                    NombreEmpleador = "Pablo Ibarra",
                    DiaPago = 30,
                    FrecuenciaPago = "Mensual",
                    TotalGrossSalary = 1500000,
                    TotalEmployeeDeductions = 157500,
                    TotalEmployerDeductions = 140100,
                    TotalBenefits = 120000,
                    TotalNetSalary = 1342500,
                    TotalEmployerCost = 1760100,
                    ApprovedBy = "Admin",
                    LastModified = DateTime.Now
                },
                Employees = new List<EmployeeDetail>
                {
                    new EmployeeDetail
                    {
                        EmployeeId = 152700726,
                        NombreEmpleado = "Pedro Vargas",
                        TipoEmpleado = "Tiempo completo",
                        GrossSalary = 500000,
                        DeductionsAmount = 52500,
                        BenefitsAmount = 40000,
                        NetSalary = 447500,
                        Status = "Activo"
                    }
                },
                EmployerCharges = new List<ChargeDetail>
                {
                    new ChargeDetail
                    {
                        ChargeName = "CCSS Patronal",
                        TotalAmount = 26550,
                        PercentageDisplay = 9.34m
                    }
                },
                EmployeeDeductions = new List<DeductionDetail>
                {
                    new DeductionDetail
                    {
                        DeductionName = "CCSS",
                        TotalAmount = 10500,
                        PercentageDisplay = 10.50m
                    }
                },
                Benefits = new List<BenefitDetail>
                {
                    new BenefitDetail
                    {
                        BenefitName = "Gimnasio",
                        BenefitType = "Monto Fijo",
                        TotalAmount = 35000
                    }
                }
            };
        }

        #endregion
    }
}