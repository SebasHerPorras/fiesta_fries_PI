using backend.Interfaces;
using backend.Models;
using backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace backend_test
{
    [TestFixture]
    public class CalculatorBenefitsServiceTests
    {
        private Mock<IEmployeeDeductionsByPayrollService> _mockEmployeeDeductionService;
        private Mock<IEmployerBenefitDeductionService> _mockEmployerBenefitDeductionService;
        private Mock<IExternalApiFactory> _mockApiFactory;
        private Mock<IEmployeeBenefitService> _mockEmployeeBenefitService;
        private CalculatorBenefitsService _service;
        private Mock<ILogger<CalculatorBenefitsService>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockEmployeeDeductionService = new Mock<IEmployeeDeductionsByPayrollService>();
            _mockEmployerBenefitDeductionService = new Mock<IEmployerBenefitDeductionService>();
            _mockApiFactory = new Mock<IExternalApiFactory>();
            _mockEmployeeBenefitService = new Mock<IEmployeeBenefitService>();
            _mockLogger = new Mock<ILogger<CalculatorBenefitsService>>();

            _service = new CalculatorBenefitsService(
                _mockEmployeeDeductionService.Object,
                _mockEmployerBenefitDeductionService.Object,
                _mockApiFactory.Object,
                _mockEmployeeBenefitService.Object,
                _mockLogger.Object
            );
        }

        #region Validaciones Básicas

        [Test]
        public void CalculateBenefitsAsync_EmpleadoNull_LanzaExcepcion()
        {
            // Arrange
            EmployeeCalculationDto? empleado = null;

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.CalculateBenefitsAsync(empleado!, 1, 3102234567)
            );
            exception.Message.Should().Contain("Los datos del empleado son requeridos");
        }

        [Test]
        public void CalculateBenefitsAsync_SalarioBrutoCero_LanzaExcepcion()
        {
            // Arrange
            var empleado = new EmployeeCalculationDto
            {
                CedulaEmpleado = 304560123,
                NombreEmpleado = "Enrique Bermúdez",
                SalarioBruto = 0,
                TipoEmpleado = "Tiempo completo",
                Cumpleanos = new DateTime(1988, 11, 15)
            };

            // Act & Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
                await _service.CalculateBenefitsAsync(empleado, 1, 3102234567)
            );
            exception.Message.Should().Contain("El salario bruto debe ser mayor a cero");
        }

        [Test]
        public async Task CalculateBenefitsAsync_SinBeneficios_RetornaCero()
        {
            // Arrange
            var empleado = CrearEmpleadoBase();
            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(new List<EmployeeBenefit>());

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(0);
            _mockEmployeeDeductionService.Verify(
                x => x.SaveEmployeeDeductions(It.IsAny<List<EmployeeDeductionsByPayrollDto>>()),
                Times.Never
            );
            _mockEmployerBenefitDeductionService.Verify(
                x => x.SaveEmployerBenefitDeductions(It.IsAny<List<EmployerBenefitDeductionDto>>()),
                Times.Never
            );
        }

        #endregion

        #region Beneficios Monto Fijo

        [Test]
        public async Task CalculateBenefitsAsync_MontoFijo_CalculaCorrectamente()
        {
            // Arrange
            var empleado = CrearEmpleadoBase();
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    EmployeeId = 304560123,
                    BenefitId = 1,
                    BenefitType = "Monto Fijo",
                    BenefitValue = 5000,
                    ApiName = "Almuerzo"
                }
            };

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(5000);
            _mockEmployerBenefitDeductionService.Verify(
                x => x.SaveEmployerBenefitDeductions(It.Is<List<EmployerBenefitDeductionDto>>(
                    list => list.Count == 1 &&
                           list[0].DeductionAmount == 5000 &&
                           list[0].BenefitName == "Almuerzo"
                )),
                Times.Once
            );
        }

        [TestCase(5000)]
        [TestCase(10000)]
        [TestCase(25000)]
        public async Task CalculateBenefitsAsync_MultiplesMontosFijos_SumaCorrectamente(decimal monto)
        {
            // Arrange
            var empleado = CrearEmpleadoBase();
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "Monto Fijo",
                    BenefitValue = monto,
                    ApiName = "Beneficio"
                }
            };

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(It.IsAny<int>()))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(monto);
        }

        #endregion

        #region Beneficios Porcentuales

        [Test]
        public async Task CalculateBenefitsAsync_Porcentual_CalculaCorrectamente()
        {
            // Arrange
            var empleado = CrearEmpleadoBase(salarioBruto: 600000);
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "Porcentual",
                    BenefitValue = 12.00m,
                    ApiName = "Seguro de Vida"
                }
            };

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            var montoEsperado = Math.Round(600000 * 0.12m, 2);
            resultado.Should().Be(montoEsperado);
            _mockEmployerBenefitDeductionService.Verify(
                x => x.SaveEmployerBenefitDeductions(It.Is<List<EmployerBenefitDeductionDto>>(
                    list => list[0].DeductionAmount == 72000 &&
                           list[0].Percentage == 12.00m
                )),
                Times.Once
            );
        }

        [TestCase(500000, 8.00, 40000)]
        [TestCase(800000, 10.50, 84000)]
        [TestCase(1200000, 5.00, 60000)]
        public async Task CalculateBenefitsAsync_Porcentual_MultiplesEscenarios(
            decimal salario, decimal porcentaje, decimal esperado)
        {
            // Arrange
            var empleado = CrearEmpleadoBase(salarioBruto: salario);
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "Porcentual",
                    BenefitValue = porcentaje,
                    ApiName = "Beneficio Porcentual"
                }
            };

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(It.IsAny<int>()))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(esperado);
        }

        #endregion

        #region Beneficios API - Asociación Solidarista

        [Test]
        public async Task CalculateBenefitsAsync_API_AsociacionSolidarista_CalculaCorrectamente()
        {
            // Arrange
            var empleado = CrearEmpleadoBase(salarioBruto: 850000);
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "API",
                    ApiName = "Asociacion Solidarista",
                    BenefitValue = null
                }
            };

            var apiResponse = new ExternalApiResponse
            {
                Deductions = new List<ApiDeduction>
                {
                    new ApiDeduction { Type = "ER", Amount = 42500 },
                    new ApiDeduction { Type = "EE", Amount = 42500 }
                }
            };

            var mockSolidarityService = new Mock<ISolidarityAssociationService>();
            mockSolidarityService
                .Setup(x => x.CalculateContributionAsync(It.IsAny<SolidarityAssociationRequest>()))
                .ReturnsAsync(apiResponse);

            _mockApiFactory
                .Setup(x => x.CreateSolidarityAssociationService())
                .Returns(mockSolidarityService.Object);

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(42500);

            _mockEmployeeDeductionService.Verify(
                x => x.SaveEmployeeDeductions(It.Is<List<EmployeeDeductionsByPayrollDto>>(
                    list => list.Count == 1 &&
                           list[0].DeductionAmount == 42500
                )),
                Times.Once
            );

            _mockEmployerBenefitDeductionService.Verify(
                x => x.SaveEmployerBenefitDeductions(It.Is<List<EmployerBenefitDeductionDto>>(
                    list => list.Count == 1 &&
                           list[0].DeductionAmount == 42500
                )),
                Times.Once
            );
        }

        #endregion

        #region Beneficios API - Seguro Privado

        [Test]
        public async Task CalculateBenefitsAsync_API_SeguroPrivado_CalculaCorrectamente()
        {
            // Arrange
            var empleado = CrearEmpleadoBase();
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "API",
                    ApiName = "Seguro Privado",
                    DependentsCount = 2
                }
            };

            var apiResponse = new ExternalApiResponse
            {
                Deductions = new List<ApiDeduction>
                {
                    new ApiDeduction { Type = "ER", Amount = 15000 }
                }
            };

            var mockInsuranceService = new Mock<IPrivateInsuranceService>();
            mockInsuranceService
                .Setup(x => x.CalculatePremiumAsync(It.Is<PrivateInsuranceRequest>(
                    req => req.DependentsCount == 2
                )))
                .ReturnsAsync(apiResponse);

            _mockApiFactory
                .Setup(x => x.CreatePrivateInsuranceService())
                .Returns(mockInsuranceService.Object);

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(15000);
            mockInsuranceService.Verify(
                x => x.CalculatePremiumAsync(It.Is<PrivateInsuranceRequest>(
                    req => req.BirthDate == empleado.Cumpleanos &&
                           req.DependentsCount == 2
                )),
                Times.Once
            );
        }

        #endregion

        #region Beneficios API - Pensiones Voluntarias

        [TestCase("A", 600000, 30000)]
        [TestCase("B", 800000, 48000)]
        [TestCase("C", 1000000, 70000)]
        public async Task CalculateBenefitsAsync_API_PensionesVoluntarias_DiferentesPlanTypes(
            string planType, decimal salario, decimal montoEsperado)
        {
            // Arrange
            var empleado = CrearEmpleadoBase(salarioBruto: salario);
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "API",
                    ApiName = "Pensiones Voluntarias",
                    PensionType = planType[0]
                }
            };

            var apiResponse = new ExternalApiResponse
            {
                Deductions = new List<ApiDeduction>
                {
                    new ApiDeduction { Type = "ER", Amount = montoEsperado }
                }
            };

            var mockPensionsService = new Mock<IVoluntaryPensionsService>();
            mockPensionsService
                .Setup(x => x.CalculatePremiumAsync(It.Is<VoluntaryPensionsRequest>(
                    req => req.PlanType == planType && req.GrossSalary == salario
                )))
                .ReturnsAsync(apiResponse);

            _mockApiFactory
                .Setup(x => x.CreateVoluntaryPensionsService())
                .Returns(mockPensionsService.Object);

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(It.IsAny<int>()))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(montoEsperado);
        }

        #endregion

        #region Beneficios Múltiples

        [Test]
        public async Task CalculateBenefitsAsync_MultipleBeneficios_CalculaSumaTotal()
        {
            // Arrange
            var empleado = CrearEmpleadoBase(salarioBruto: 600000);
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "Monto Fijo",
                    BenefitValue = 5000,
                    ApiName = "Almuerzo"
                },
                new EmployeeBenefit
                {
                    BenefitType = "Porcentual",
                    BenefitValue = 10.00m,
                    ApiName = "Bonificación"
                },
                new EmployeeBenefit
                {
                    BenefitType = "Monto Fijo",
                    BenefitValue = 10000,
                    ApiName = "Capacitación"
                }
            };

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(75000);

            _mockEmployerBenefitDeductionService.Verify(
                x => x.SaveEmployerBenefitDeductions(It.Is<List<EmployerBenefitDeductionDto>>(
                    list => list.Count == 3
                )),
                Times.Once
            );
        }

        [Test]
        public async Task CalculateBenefitsAsync_MixtoConAPI_CalculaTodoCorrectamente()
        {
            // Arrange
            var empleado = CrearEmpleadoBase(salarioBruto: 850000);
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "Monto Fijo",
                    BenefitValue = 5000,
                    ApiName = "Almuerzo"
                },
                new EmployeeBenefit
                {
                    BenefitType = "API",
                    ApiName = "Asociacion Solidarista"
                }
            };

            var apiResponse = new ExternalApiResponse
            {
                Deductions = new List<ApiDeduction>
                {
                    new ApiDeduction { Type = "ER", Amount = 42500 },
                    new ApiDeduction { Type = "EE", Amount = 42500 }
                }
            };

            var mockSolidarityService = new Mock<ISolidarityAssociationService>();
            mockSolidarityService
                .Setup(x => x.CalculateContributionAsync(It.IsAny<SolidarityAssociationRequest>()))
                .ReturnsAsync(apiResponse);

            _mockApiFactory
                .Setup(x => x.CreateSolidarityAssociationService())
                .Returns(mockSolidarityService.Object);

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(47500);
        }

        #endregion

        #region Casos Edge

        [Test]
        public async Task CalculateBenefitsAsync_BenefitValueNull_NoCalculaNada()
        {
            // Arrange
            var empleado = CrearEmpleadoBase();
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "Porcentual",
                    BenefitValue = null,
                    ApiName = "Beneficio Sin Valor"
                }
            };

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(0);
        }

        [Test]
        public async Task CalculateBenefitsAsync_TipoBeneficioDesconocido_IgnoraYContinua()
        {
            // Arrange
            var empleado = CrearEmpleadoBase();
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "TIPO_DESCONOCIDO",
                    BenefitValue = 100,
                    ApiName = "Beneficio Raro"
                },
                new EmployeeBenefit
                {
                    BenefitType = "Monto Fijo",
                    BenefitValue = 5000,
                    ApiName = "Almuerzo"
                }
            };

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(5000);
        }

        [Test]
        public async Task CalculateBenefitsAsync_APINoReconocida_RetornaResponseVacio()
        {
            // Arrange
            var empleado = CrearEmpleadoBase();
            var beneficios = new List<EmployeeBenefit>
            {
                new EmployeeBenefit
                {
                    BenefitType = "API",
                    ApiName = "API_NO_EXISTE"
                }
            };

            _mockEmployeeBenefitService
                .Setup(x => x.GetSelectedByEmployeeIdAsync(304560123))
                .ReturnsAsync(beneficios);

            // Act
            var resultado = await _service.CalculateBenefitsAsync(empleado, 1, 3102234567);

            // Assert
            resultado.Should().Be(0);
        }

        #endregion

        #region Métodos Helper

        private EmployeeCalculationDto CrearEmpleadoBase(decimal salarioBruto = 600000)
        {
            return new EmployeeCalculationDto
            {
                CedulaEmpleado = 304560123,
                NombreEmpleado = "Enrique Bermúdez",
                SalarioBruto = salarioBruto,
                TipoEmpleado = "Tiempo completo",
                Cumpleanos = new DateTime(1988, 11, 15),
                horas = 160
            };
        }

        #endregion
    }
}