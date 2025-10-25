using NUnit.Framework;
using Moq;
using FluentAssertions;
using backend.Services;
using backend.Models;
using backend.Interfaces;

namespace backend_test
{
    [TestFixture]
    public class CalculatorDeductionsEmployerServiceTests
    {
        private ICalculatorDeductionsEmployerService _service;
        private Mock<IEmployerSocialSecurityContributionsService> _mockContributionsService;
        private Mock<IEmployerSocialSecurityByPayrollService> _mockPayrollService;
        private List<EmployerSocialSecurityContributions> _testContributions;

        [SetUp]
        public void Setup()
        {
            // Crear mocks de las dependencias
            _mockContributionsService = new Mock<IEmployerSocialSecurityContributionsService>();
            _mockPayrollService = new Mock<IEmployerSocialSecurityByPayrollService>();

            // Datos de prueba para cargas sociales
            _testContributions = new List<EmployerSocialSecurityContributions>
            {
                new EmployerSocialSecurityContributions { Name = "CCSS Salud", Percentage = 0.0925m },
                new EmployerSocialSecurityContributions { Name = "CCSS Pensiones", Percentage = 0.0525m },
                new EmployerSocialSecurityContributions { Name = "INA", Percentage = 0.005m },
                new EmployerSocialSecurityContributions { Name = "ASFA", Percentage = 0.0025m }
            };

            // Configurar el mock para devolver los datos de prueba
            _mockContributionsService
                .Setup(x => x.GetActiveContributions())
                .Returns(_testContributions);

            // Configurar el mock del payroll service para no hacer nada
            _mockPayrollService
                .Setup(x => x.SaveEmployerDeductions(It.IsAny<List<EmployerSocialSecurityByPayrollDto>>()));

            // GENERAR EL OBJETO CONCRETO INTERNAMENTE usando la interfaz
            _service = CreateServiceInstance();
        }

        private ICalculatorDeductionsEmployerService CreateServiceInstance()
        {
            // Crear la implementación concreta con las dependencias mockeadas
            return new CalculatorDeductionsEmployerService(
                _mockContributionsService.Object,
                _mockPayrollService.Object);
        }

        [Test]
        public void CalculateEmployerDeductions_ValidEmployee_ReturnsCorrectTotal()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 123456789,
                NombreEmpleado = "Test Employee",
                SalarioBruto = 500000m,
                TipoEmpleado = "Tiempo Completo"
            };

            var reportId = 1;
            var companyLegalId = 3101123456L;

            // Expected: 46250 + 26250 + 2500 + 1250 = 76250
            var expectedTotal = 76250m;

            // Act
            var result = _service.CalculateEmployerDeductions(employee, reportId, companyLegalId);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public void CalculateEmployerDeductions_NullEmployee_ThrowsArgumentException()
        {
            // Arrange
            EmployeeCalculationDto employee = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _service.CalculateEmployerDeductions(employee, 1, 3101123456L));
            
            exception.Message.Should().Contain("Los datos del empleado son requeridos");
        }

        [Test]
        public void CalculateEmployerDeductions_ZeroSalary_ThrowsArgumentException()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 123456789,
                NombreEmpleado = "Test Employee",
                SalarioBruto = 0m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _service.CalculateEmployerDeductions(employee, 1, 3101123456L));
            
            exception.Message.Should().Contain("El salario bruto debe ser mayor a cero");
        }

        [Test]
        public void CalculateEmployerDeductions_EmptyContributions_ThrowsArgumentException()
        {
            // Arrange - Configurar mock para devolver lista vacía
            _mockContributionsService
                .Setup(x => x.GetActiveContributions())
                .Returns(new List<EmployerSocialSecurityContributions>());

            // Recrear el servicio con la nueva configuración
            _service = CreateServiceInstance();

            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 123456789,
                NombreEmpleado = "Test Employee",
                SalarioBruto = 500000m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _service.CalculateEmployerDeductions(employee, 1, 3101123456L));
            
            exception.Message.Should().Contain("La lista de cargas sociales es requerida");
        }

        [Test]
        public void CalculateEmployerDeductions_HighSalary_CalculatesCorrectly()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 987654321,
                NombreEmpleado = "High Earner",
                SalarioBruto = 2000000m,
                TipoEmpleado = "Gerencia"
            };

            // Expected: 185000 + 105000 + 10000 + 5000 = 305000
            var expectedTotal = 305000m;

            // Act
            var result = _service.CalculateEmployerDeductions(employee, 2, 3102234567L);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public void CalculateEmployerDeductions_CallsPayrollServiceCorrectly()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 123456789,
                NombreEmpleado = "Test Employee",
                SalarioBruto = 500000m,
                TipoEmpleado = "Tiempo Completo"
            };

            var reportId = 1;
            var companyLegalId = 3101123456L;

            // Act
            _service.CalculateEmployerDeductions(employee, reportId, companyLegalId);

            // Assert - Verificar que se llamó el método con los parámetros correctos
            _mockPayrollService.Verify(x => x.SaveEmployerDeductions(
                It.Is<List<EmployerSocialSecurityByPayrollDto>>(deductions => 
                    deductions.Count == 4 &&
                    deductions.All(d => d.ReportId == reportId) &&
                    deductions.All(d => d.EmployeeId == employee.CedulaEmpleado) &&
                    deductions.All(d => d.CedulaJuridicaEmpresa == companyLegalId) &&
                    deductions.Any(d => d.ChargeName == "CCSS Salud" && d.Amount == 46250m)
                )), Times.Once);
        }

        [Test]
        public void ObtenerCargasSocialesActuales_ReturnsCurrentContributions()
        {
            // Act
            var result = _service.ObtenerCargasSocialesActuales();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result.Should().Contain(c => c.Name == "CCSS Salud" && c.Percentage == 0.0925m);
            result.Should().Contain(c => c.Name == "CCSS Pensiones" && c.Percentage == 0.0525m);
            result.Should().Contain(c => c.Name == "INA" && c.Percentage == 0.005m);
            result.Should().Contain(c => c.Name == "ASFA" && c.Percentage == 0.0025m);
        }

        [Test]
        public void CalculateEmployerDeductions_DecimalSalary_RoundsCorrectly()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 555555555,
                NombreEmpleado = "Decimal Test",
                SalarioBruto = 333333.33m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Act
            var result = _service.CalculateEmployerDeductions(employee, 1, 3101123456L);

            // Assert - Verificar que está redondeado a 2 decimales
            result.Should().NotBe(0);
            var rounded = Math.Round(result, 2);
            result.Should().Be(rounded);
        }

        [Test]
        public void CalculateEmployerDeductions_DifferentCompanies_HandlesCorrectly()
        {
            // Arrange
            var employee1 = new EmployeeCalculationDto
            {
                CedulaEmpleado = 111111111,
                NombreEmpleado = "Employee 1",
                SalarioBruto = 400000m,
                TipoEmpleado = "Tiempo Completo"
            };

            var employee2 = new EmployeeCalculationDto
            {
                CedulaEmpleado = 222222222,
                NombreEmpleado = "Employee 2",
                SalarioBruto = 600000m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Act
            var result1 = _service.CalculateEmployerDeductions(employee1, 1, 3101123456L);
            var result2 = _service.CalculateEmployerDeductions(employee2, 1, 3102234567L);

            // Assert
            result1.Should().Be(61000m); // 400000 * 0.1525 = 61000
            result2.Should().Be(91500m);  // 600000 * 0.1525 = 91500

            // Verificar que se llamó dos veces al payroll service
            _mockPayrollService.Verify(x => x.SaveEmployerDeductions(
                It.IsAny<List<EmployerSocialSecurityByPayrollDto>>()), Times.Exactly(2));
        }
    }
}