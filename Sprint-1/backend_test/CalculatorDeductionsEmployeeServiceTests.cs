using NUnit.Framework;
using Moq;
using FluentAssertions;
using backend.Services;
using backend.Models;
using backend.Interfaces;

namespace backend_test
{
    [TestFixture]
    public class CalculatorDeductionsEmployeeServiceTests
    {
        private ICalculatorDeductionsEmployeeService _service;
        private Mock<IEmployeeSocialSecurityContributionsService> _mockSocialSecurityService;
        private Mock<IPersonalIncomeTaxService> _mockIncomeTaxService;
        private Mock<IEmployeeDeductionsByPayrollService> _mockPayrollService;
        private List<EmployeeSocialSecurityContributions> _testSocialContributions;
        private List<PersonalIncomeTax> _testTaxScales;

        [SetUp]
        public void Setup()
        {
            // Crear mocks de las dependencias
            _mockSocialSecurityService = new Mock<IEmployeeSocialSecurityContributionsService>();
            _mockIncomeTaxService = new Mock<IPersonalIncomeTaxService>();
            _mockPayrollService = new Mock<IEmployeeDeductionsByPayrollService>();

            // Datos de prueba para deducciones sociales del empleado
            _testSocialContributions = new List<EmployeeSocialSecurityContributions>
            {
                new EmployeeSocialSecurityContributions 
                { 
                    Id = 1,
                    Name = "CCSS Salud Empleado", 
                    Percentage = 0.055m,
                    Active = true 
                },
                new EmployeeSocialSecurityContributions 
                { 
                    Id = 2,
                    Name = "CCSS Pensiones Empleado (IVM)", 
                    Percentage = 0.04m,
                    Active = true 
                }
            };

            // Datos de prueba para escalas de impuesto sobre la renta
            _testTaxScales = new List<PersonalIncomeTax>
            {
                new PersonalIncomeTax 
                { 
                    Id = 1,
                    Name = "Escala 1", 
                    MinAmount = 0.00m, 
                    MaxAmount = 922000.00m, 
                    Percentage = 0.00m, 
                    BaseAmount = 0.00m,
                    Active = true 
                },
                new PersonalIncomeTax 
                { 
                    Id = 2,
                    Name = "Escala 2", 
                    MinAmount = 922001.00m, 
                    MaxAmount = 1352000.00m, 
                    Percentage = 0.10m, 
                    BaseAmount = 0.00m,
                    Active = true 
                },
                new PersonalIncomeTax 
                { 
                    Id = 3,
                    Name = "Escala 3", 
                    MinAmount = 1352001.00m, 
                    MaxAmount = 2373000.00m, 
                    Percentage = 0.15m, 
                    BaseAmount = 43000.00m,
                    Active = true 
                },
                new PersonalIncomeTax 
                { 
                    Id = 4,
                    Name = "Escala 4", 
                    MinAmount = 2373001.00m, 
                    MaxAmount = 4745000.00m, 
                    Percentage = 0.20m, 
                    BaseAmount = 196150.00m,
                    Active = true 
                },
                new PersonalIncomeTax 
                { 
                    Id = 5,
                    Name = "Escala 5", 
                    MinAmount = 4745001.00m, 
                    MaxAmount = null, 
                    Percentage = 0.25m, 
                    BaseAmount = 670550.00m,
                    Active = true 
                }
            };

            // Configurar mocks
            _mockSocialSecurityService
                .Setup(x => x.GetActiveContributions())
                .Returns(_testSocialContributions);

            _mockIncomeTaxService
                .Setup(x => x.GetActiveScales())
                .Returns(_testTaxScales);

            _mockPayrollService
                .Setup(x => x.SaveEmployeeDeductions(It.IsAny<List<EmployeeDeductionsByPayrollDto>>()));

            // Crear el servicio con dependencias mockeadas
            _service = CreateServiceInstance();
        }

        private ICalculatorDeductionsEmployeeService CreateServiceInstance(bool saveInDB = true)
        {
            return new CalculatorDeductionsEmployeeService(
                _mockSocialSecurityService.Object,
                _mockIncomeTaxService.Object,
                _mockPayrollService.Object,
                saveInDB);
        }

        [Test]
        public void CalculateEmployeeDeductions_ValidEmployee_LowSalary_ReturnsCorrectTotal()
        {
            // Arrange - Diego Cerdas con salario bajo (no paga impuesto)
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 119180741,
                NombreEmpleado = "Diego Cerdas Delgado",
                SalarioBruto = 450000m,
                TipoEmpleado = "Tiempo completo"
            };

            var reportId = 1;
            var companyLegalId = 3102234567L;

            // Expected: CCSS Salud (24750) + CCSS Pensiones (18000) + IR (0) = 42750
            var expectedTotal = 42750m;

            // Act
            var result = _service.CalculateEmployeeDeductions(employee, reportId, companyLegalId);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public void CalculateEmployeeDeductions_ValidEmployee_HighSalary_ReturnsCorrectTotal()
        {
            // Arrange - Empleado con salario alto (paga impuesto - Escala 3)
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 999888777,
                NombreEmpleado = "Ignacio Pérez",
                SalarioBruto = 2000000m,
                TipoEmpleado = "Tiempo completo"
            };

            var reportId = 1;
            var companyLegalId = 3102234567L;

            // Expected: 
            // CCSS Salud: 2000000 * 0.055 = 110000
            // CCSS Pensiones: 2000000 * 0.04 = 80000
            // IR: (2000000 - 1352001) * 0.15 + 43000 = 647999 * 0.15 + 43000 = 97199.85 + 43000 = 140199.85
            // Total: 110000 + 80000 + 140199.85 = 330199.85
            var expectedTotal = 330199.85m;

            // Act
            var result = _service.CalculateEmployeeDeductions(employee, reportId, companyLegalId);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public void CalculateEmployeeDeductions_NullEmployee_ThrowsArgumentException()
        {
            // Arrange
            EmployeeCalculationDto employee = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _service.CalculateEmployeeDeductions(employee, 1, 3102234567L));
            
            exception.Message.Should().Contain("Los datos del empleado son requeridos");
        }

        [Test]
        public void CalculateEmployeeDeductions_ZeroSalary_ThrowsArgumentException()
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
                _service.CalculateEmployeeDeductions(employee, 1, 3102234567L));
            
            exception.Message.Should().Contain("El salario bruto debe ser mayor a cero");
        }

        [Test]
        public void CalculateEmployeeDeductions_NegativeSalary_ThrowsArgumentException()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 123456789,
                NombreEmpleado = "Test Employee",
                SalarioBruto = -1000m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _service.CalculateEmployeeDeductions(employee, 1, 3102234567L));
            
            exception.Message.Should().Contain("El salario bruto debe ser mayor a cero");
        }

        [Test]
        public void CalculateEmployeeDeductions_BoundaryTaxScale1_CalculatesCorrectly()
        {
            // Arrange - Salario en el límite superior de Escala 1
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 111111111,
                NombreEmpleado = "Boundary Test 1",
                SalarioBruto = 922000m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Expected: CCSS Salud (50710) + CCSS Pensiones (36880) + IR (0) = 87590
            var expectedTotal = 87590m;

            // Act
            var result = _service.CalculateEmployeeDeductions(employee, 1, 3102234567L);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public void CalculateEmployeeDeductions_BoundaryTaxScale2_CalculatesCorrectly()
        {
            // Arrange - Salario en el límite inferior de Escala 2
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 222222222,
                NombreEmpleado = "Boundary Test 2",
                SalarioBruto = 922001m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Expected: 
            // CCSS Salud: 922001 * 0.055 = 50710.06
            // CCSS Pensiones: 922001 * 0.04 = 36880.04
            // IR: (922001 - 922001) * 0.10 + 0 = 0
            // Total: 50710.06 + 36880.04 + 0 = 87590.10
            var expectedTotal = 87590.10m;

            // Act
            var result = _service.CalculateEmployeeDeductions(employee, 1, 3102234567L);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public void CalculateEmployeeDeductions_CallsPayrollServiceCorrectly()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 123456789,
                NombreEmpleado = "Test Employee",
                SalarioBruto = 1500000m,
                TipoEmpleado = "Tiempo Completo"
            };

            var reportId = 5;
            var companyLegalId = 3101123456L;

            // Act
            _service.CalculateEmployeeDeductions(employee, reportId, companyLegalId);

            // Assert - Verificar que se llamó el método con los parámetros correctos
            _mockPayrollService.Verify(x => x.SaveEmployeeDeductions(
                It.Is<List<EmployeeDeductionsByPayrollDto>>(deductions => 
                    deductions.Count == 3 && // 2 CCSS + 1 IR
                    deductions.All(d => d.ReportId == reportId) &&
                    deductions.All(d => d.EmployeeId == employee.CedulaEmpleado) &&
                    deductions.All(d => d.CedulaJuridicaEmpresa == companyLegalId) &&
                    deductions.Any(d => d.DeductionName == "CCSS Salud Empleado") &&
                    deductions.Any(d => d.DeductionName == "CCSS Pensiones Empleado (IVM)") &&
                    deductions.Any(d => d.DeductionName == "Impuesto sobre la Renta")
                )), Times.Once);
        }

        [Test]
        public void CalculateEmployeeDeductions_NoIncomeTax_OnlyAddsCCSSDeductions()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 123456789,
                NombreEmpleado = "Low Earner",
                SalarioBruto = 500000m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Act
            _service.CalculateEmployeeDeductions(employee, 1, 3102234567L);

            // Assert - Solo debe agregar 2 deducciones CCSS, no impuesto sobre la renta
            _mockPayrollService.Verify(x => x.SaveEmployeeDeductions(
                It.Is<List<EmployeeDeductionsByPayrollDto>>(deductions => 
                    deductions.Count == 2 &&
                    deductions.All(d => d.DeductionName.Contains("CCSS")) &&
                    deductions.All(d => d.Percentage.HasValue)
                )), Times.Once);
        }

        [Test]
        public void ObtenerDeduccionesSocialesActuales_ReturnsCurrentContributions()
        {
            // Act
            var result = _service.ObtenerDeduccionesSocialesActuales();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Name == "CCSS Salud Empleado" && c.Percentage == 0.055m);
            result.Should().Contain(c => c.Name == "CCSS Pensiones Empleado (IVM)" && c.Percentage == 0.04m);
        }

        [Test]
        public void ObtenerEscalasImpuestoRenta_ReturnsCurrentScales()
        {
            // Act
            var result = _service.ObtenerEscalasImpuestoRenta();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result.Should().Contain(s => s.Name == "Escala 1" && s.Percentage == 0.00m);
            result.Should().Contain(s => s.Name == "Escala 2" && s.Percentage == 0.10m);
            result.Should().Contain(s => s.Name == "Escala 3" && s.Percentage == 0.15m);
            result.Should().Contain(s => s.Name == "Escala 4" && s.Percentage == 0.20m);
            result.Should().Contain(s => s.Name == "Escala 5" && s.Percentage == 0.25m);
        }

        [Test]
        public void CalculateEmployeeDeductions_DecimalSalary_RoundsCorrectly()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 555555555,
                NombreEmpleado = "Decimal Test",
                SalarioBruto = 987654.32m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Act
            var result = _service.CalculateEmployeeDeductions(employee, 1, 3102234567L);

            // Assert - Verificar que está redondeado a 2 decimales
            result.Should().NotBe(0);
            var rounded = Math.Round(result, 2);
            result.Should().Be(rounded);
        }

        [Test]
        public void CalculateEmployeeDeductions_HighestTaxScale_CalculatesCorrectly()
        {
            // Arrange - Salario muy alto en Escala 5
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 777777777,
                NombreEmpleado = "High Earner",
                SalarioBruto = 5000000m,
                TipoEmpleado = "Ejecutivo"
            };

            // Expected:
            // CCSS Salud: 5000000 * 0.055 = 275000
            // CCSS Pensiones: 5000000 * 0.04 = 200000
            // IR: (5000000 - 4745001) * 0.25 + 670550 = 254999 * 0.25 + 670550 = 63749.75 + 670550 = 734299.75
            // Total: 275000 + 200000 + 734299.75 = 1209299.75
            var expectedTotal = 1209299.75m;

            // Act
            var result = _service.CalculateEmployeeDeductions(employee, 1, 3102234567L);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public void CalculateEmployeeDeductions_EmptyTaxScales_OnlyCalculatesCCSS()
        {
            // Arrange - Configurar mock para devolver lista vacía de escalas
            _mockIncomeTaxService
                .Setup(x => x.GetActiveScales())
                .Returns(new List<PersonalIncomeTax>());

            // Recrear el servicio con la nueva configuración
            var serviceWithEmptyScales = CreateServiceInstance();

            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 123456789,
                NombreEmpleado = "Test Employee",
                SalarioBruto = 2000000m,
                TipoEmpleado = "Tiempo Completo"
            };

            // Expected: Solo CCSS = 110000 + 80000 = 190000
            var expectedTotal = 190000m;

            // Act
            var result = serviceWithEmptyScales.CalculateEmployeeDeductions(employee, 1, 3102234567L);

            // Assert
            result.Should().Be(expectedTotal);
        }
        [Test]
        public void CalculateEmployeeDeductions_HourlyEmployee_ReturnsZeroDeductions()
        {
            // Arrange - Empleado por horas (servicio profesional)
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 555666777,
                NombreEmpleado = "María González",
                SalarioBruto = 800000m,
                TipoEmpleado = "Por horas"
            };

            var reportId = 1;
            var companyLegalId = 3102234567L;

            // Expected: 0 deducciones para empleados por horas
            var expectedTotal = 0m;

            // Act
            var result = _service.CalculateEmployeeDeductions(employee, reportId, companyLegalId);

            // Assert
            result.Should().Be(expectedTotal);
        }

        [Test]
        public void CalculateEmployeeDeductions_HourlyEmployee_SavesInformativeRecord()
        {
            // Arrange
            var employee = new EmployeeCalculationDto
            {
                CedulaEmpleado = 888999000,
                NombreEmpleado = "Carlos Freelancer",
                SalarioBruto = 1500000m,
                TipoEmpleado = "Por horas"
            };

            var reportId = 2;
            var companyLegalId = 3101123456L;

            // Act
            _service.CalculateEmployeeDeductions(employee, reportId, companyLegalId);

            // Assert - Verificar que se guardó solo un registro informativo
            _mockPayrollService.Verify(x => x.SaveEmployeeDeductions(
                It.Is<List<EmployeeDeductionsByPayrollDto>>(deductions =>
                    deductions.Count == 1 &&
                    deductions[0].ReportId == reportId &&
                    deductions[0].EmployeeId == employee.CedulaEmpleado &&
                    deductions[0].CedulaJuridicaEmpresa == companyLegalId &&
                    deductions[0].DeductionName == "Sin deducciones - Empleado por horas" &&
                    deductions[0].DeductionAmount == 0 &&
                    deductions[0].Percentage == null
                )), Times.Once);
        }
    }
}