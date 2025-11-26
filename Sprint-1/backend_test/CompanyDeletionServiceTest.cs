using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace backend.Tests.Services
{
    // ---------- MODELOS ----------
    public class EmpresaModel
    {
        public long CedulaJuridica { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class EmpleadoListDto
    {
        public string Cedula { get; set; } = string.Empty;
    }

    public class BeneficioModel
    {
        public int IdBeneficio { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CompanyDeletionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int EmployeesProcessed { get; set; }
        public int SuccessfulDeletions { get; set; }
        public int BenefitsProcessed { get; set; }
        public int SuccessfulBenefitDeletions { get; set; }
    }

    // ---------- INTERFACES ----------
    public interface IEmpleadoRepository
    {
        List<EmpleadoListDto> GetByEmpresa(long cedulaJuridica);
    }

    public interface IEmpresaRepository
    {
        EmpresaModel? GetByCedulaJuridica(long cedulaJuridica);
        int CheckCompanyPayroll(EmpresaModel empresa);
        Task DeleteEmpresa(long cedulaJuridica, bool logical);
    }

    public interface IEmployeeDeletionRepository
    {
        Task<bool> ExecutePhysicalDeleteAsync(string cedula);
        Task<bool> ExecuteLogicalDeleteAsync(string cedula);
    }

    public interface IBeneficioRepository
    {
        List<BeneficioModel> GetByEmpresa(long cedulaJuridica);
        void PhysicalDeletion(int idBeneficio);
        void LogicalDeletion(int idBeneficio, DateTime? lastPeriod);
        bool ExistsInEmployerBenefitDeductions(int idBeneficio);
    }

    // ---------- SERVICIO ----------
    public class CompanyDeletionServiceTest
    {
        private readonly ILogger<CompanyDeletionServiceTest>? _logger;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IEmployeeDeletionRepository _employeeDeletionRepository;
        private readonly IBeneficioRepository _beneficioRepository;

        public CompanyDeletionServiceTest(
            ILogger<CompanyDeletionServiceTest>? logger,
            IEmpleadoRepository empleadoRepository,
            IEmpresaRepository empresaRepository,
            IEmployeeDeletionRepository employeeDeletionRepository,
            IBeneficioRepository beneficioRepository)
        {
            _logger = logger;
            _empleadoRepository = empleadoRepository;
            _empresaRepository = empresaRepository;
            _employeeDeletionRepository = employeeDeletionRepository;
            _beneficioRepository = beneficioRepository;
        }

        public async Task<CompanyDeletionResult> DeleteCompany(long cedulaJuridica)
        {
            var result = new CompanyDeletionResult();

            try
            {
                var empresa = _empresaRepository.GetByCedulaJuridica(cedulaJuridica);
                if (empresa == null)
                {
                    result.Success = false;
                    result.Message = "No se encontró la empresa";
                    return result;
                }

                // 0 = no payroll, 1 = has payroll
                var hasPayroll = _empresaRepository.CheckCompanyPayroll(empresa);

                // Empleados
                var empleados = _empleadoRepository.GetByEmpresa(cedulaJuridica);
                result.EmployeesProcessed = empleados?.Count ?? 0;

                int successEmp = 0;
                if (empleados != null)
                {
                    foreach (var e in empleados)
                    {
                        try
                        {
                            bool deleted;
                            if (hasPayroll == 0)
                                deleted = await _employeeDeletionRepository.ExecutePhysicalDeleteAsync(e.Cedula);
                            else
                                deleted = await _employeeDeletionRepository.ExecuteLogicalDeleteAsync(e.Cedula);

                            if (deleted) successEmp++;
                        }
                        catch (Exception ex)
                        {
                            // Registrar y continuar
                            _logger?.LogWarning(ex, "Error eliminando empleado {Cedula}", e.Cedula);
                        }
                    }
                }
                result.SuccessfulDeletions = successEmp;

                // Beneficios
                var beneficios = _beneficioRepository.GetByEmpresa(cedulaJuridica);
                result.BenefitsProcessed = beneficios?.Count ?? 0;

                int successBen = 0;
                if (beneficios != null)
                {
                    foreach (var b in beneficios)
                    {
                        try
                        {
                            if (hasPayroll == 0)
                            {
                                // physical
                                _beneficio_repository_physical_call(b);
                                successBen++;
                            }
                            else
                            {
                                // logical: if exists in deductions
                                bool hasDeductions = _beneficioRepository.ExistsInEmployerBenefitDeductions(b.IdBeneficio);
                                DateTime? lastPeriod = hasDeductions ? DateTime.UtcNow : (DateTime?)null;
                                _beneficioRepository.LogicalDeletion(b.IdBeneficio, lastPeriod);
                                successBen++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Error eliminando beneficio {Id}", b.IdBeneficio);
                        }
                    }
                }
                result.SuccessfulBenefitDeletions = successBen;

                bool overallSuccess = true;
                if (result.EmployeesProcessed != result.SuccessfulDeletions) overallSuccess = false;
                if (result.BenefitsProcessed != result.SuccessfulBenefitDeletions) overallSuccess = false;

                // Elimina la compañia (el lógico depende del payroll)
                await _empresaRepository.DeleteEmpresa(cedulaJuridica, hasPayroll == 1);

                result.Success = overallSuccess;
                result.Message = overallSuccess ? "Eliminación completada" : "Eliminación parcial";
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno en DeleteCompany");
                return new CompanyDeletionResult
                {
                    Success = false,
                    Message = $"Error interno: {ex.Message}"
                };
            }
        }

        // wrapper para poder mockear o aislar llamadas de PhysicalDeletion
        private void _beneficio_repository_physical_call(BeneficioModel b)
        {
            _beneficioRepository.PhysicalDeletion(b.IdBeneficio);
        }
    }

    // ---------- TESTS ---------
    public class CompanyDeletionServiceTests
    {
        private readonly Mock<ILogger<CompanyDeletionServiceTest>> _loggerMock;
        private readonly Mock<IEmpleadoRepository> _empleadoRepositoryMock;
        private readonly Mock<IEmpresaRepository> _empresaRepositoryMock;
        private readonly Mock<IEmployeeDeletionRepository> _employeeDeletionRepositoryMock;
        private readonly Mock<IBeneficioRepository> _beneficioRepositoryMock;
        private readonly CompanyDeletionServiceTest _service;

        public CompanyDeletionServiceTests()
        {
            _loggerMock = new Mock<ILogger<CompanyDeletionServiceTest>>();
            _empleadoRepositoryMock = new Mock<IEmpleadoRepository>();
            _empresa_repository_mock_init();
            _empresaRepositoryMock = new Mock<IEmpresaRepository>();
            _employeeDeletionRepositoryMock = new Mock<IEmployeeDeletionRepository>();
            _beneficioRepositoryMock = new Mock<IBeneficioRepository>();

            _service = new CompanyDeletionServiceTest(
                _loggerMock.Object,
                _empleadoRepositoryMock.Object,
                _empresaRepositoryMock.Object,
                _employeeDeletionRepositoryMock.Object,
                _beneficioRepositoryMock.Object);
        }

        private void _empresa_repository_mock_init()
        {
            // placeholder if se necesita inicializar algo adicional
        }

        [Fact]
        public async Task DeleteCompany_WhenCompanyNotFound_ReturnsFailureResult()
        {
            var cedulaJuridica = 123456789L;
            _empresaRepositoryMock.Setup(x => x.GetByCedulaJuridica(cedulaJuridica)).Returns((EmpresaModel?)null);

            var result = await _service.DeleteCompany(cedulaJuridica);

            Assert.False(result.Success);
            Assert.Equal("No se encontró la empresa", result.Message);
            Assert.Equal(0, result.EmployeesProcessed);
            Assert.Equal(0, result.SuccessfulDeletions);
            Assert.Equal(0, result.BenefitsProcessed);
            Assert.Equal(0, result.SuccessfulBenefitDeletions);
        }

        [Fact]
        public async Task DeleteCompany_WhenNoPayroll_DeletesEmployeesAndBenefitsPhysically()
        {
            var cedulaJuridica = 123456789L;
            var empresa = new EmpresaModel { CedulaJuridica = cedulaJuridica, Nombre = "Test Company" };
            var employees = new List<EmpleadoListDto>
            {
                new EmpleadoListDto { Cedula = "111111111" },
                new EmpleadoListDto { Cedula = "222222222" }
            };
            var benefits = new List<BeneficioModel>
            {
                new BeneficioModel { IdBeneficio = 1, Nombre = "Benefit 1" },
                new BeneficioModel { IdBeneficio = 2, Nombre = "Benefit 2" }
            };

            _empresaRepositoryMock.Setup(x => x.GetByCedulaJuridica(cedulaJuridica)).Returns(empresa);
            _empresaRepositoryMock.Setup(x => x.CheckCompanyPayroll(empresa)).Returns(0); // No payroll
            _empleadoRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(employees);
            _beneficioRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(benefits);
            _employeeDeletionRepositoryMock.Setup(x => x.ExecutePhysicalDeleteAsync(It.IsAny<string>())).ReturnsAsync(true);
            _beneficioRepositoryMock.Setup(x => x.PhysicalDeletion(It.IsAny<int>()));
            _empresaRepositoryMock.Setup(x => x.DeleteEmpresa(cedulaJuridica, false)).Returns(Task.CompletedTask);

            var result = await _service.DeleteCompany(cedulaJuridica);

            Assert.True(result.Success);
            Assert.Equal(2, result.EmployeesProcessed);
            Assert.Equal(2, result.SuccessfulDeletions);
            Assert.Equal(2, result.BenefitsProcessed);
            Assert.Equal(2, result.SuccessfulBenefitDeletions);

            _employeeDeletionRepositoryMock.Verify(x => x.ExecutePhysicalDeleteAsync("111111111"), Times.Once);
            _employeeDeletionRepositoryMock.Verify(x => x.ExecutePhysicalDeleteAsync("222222222"), Times.Once);
            _beneficioRepositoryMock.Verify(x => x.PhysicalDeletion(1), Times.Once);
            _beneficioRepositoryMock.Verify(x => x.PhysicalDeletion(2), Times.Once);
        }

        [Fact]
        public async Task DeleteCompany_WithPayroll_DeletesEmployeesAndBenefitsLogically()
        {
            var cedulaJuridica = 123456789L;
            var empresa = new EmpresaModel { CedulaJuridica = cedulaJuridica, Nombre = "Test Company" };
            var employees = new List<EmpleadoListDto>
            {
                new EmpleadoListDto { Cedula = "111111111" }
            };
            var benefits = new List<BeneficioModel>
            {
                new BeneficioModel { IdBeneficio = 1, Nombre = "Benefit 1" }
            };

            _empresaRepositoryMock.Setup(x => x.GetByCedulaJuridica(cedulaJuridica)).Returns(empresa);
            _empresaRepositoryMock.Setup(x => x.CheckCompanyPayroll(empresa)).Returns(1); // Has payroll
            _empleadoRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(employees);
            _beneficioRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(benefits);
            _employeeDeletionRepositoryMock.Setup(x => x.ExecuteLogicalDeleteAsync(It.IsAny<string>())).ReturnsAsync(true);
            _beneficioRepositoryMock.Setup(x => x.ExistsInEmployerBenefitDeductions(It.IsAny<int>())).Returns(false);
            _beneficioRepositoryMock.Setup(x => x.LogicalDeletion(It.IsAny<int>(), It.IsAny<DateTime?>()));
            _empresaRepositoryMock.Setup(x => x.DeleteEmpresa(cedulaJuridica, true)).Returns(Task.CompletedTask);

            var result = await _service.DeleteCompany(cedulaJuridica);

            Assert.True(result.Success);
            Assert.Equal(1, result.EmployeesProcessed);
            Assert.Equal(1, result.SuccessfulDeletions);
            Assert.Equal(1, result.BenefitsProcessed);
            Assert.Equal(1, result.SuccessfulBenefitDeletions);

            _employeeDeletionRepositoryMock.Verify(x => x.ExecuteLogicalDeleteAsync("111111111"), Times.Once);
            _beneficioRepositoryMock.Verify(x => x.LogicalDeletion(1, null), Times.Once);
        }

        [Fact]
        public async Task DeleteCompany_WhenEmployeeDeletionFails_ReturnsPartialSuccess()
        {
            var cedulaJuridica = 123456789L;
            var empresa = new EmpresaModel { CedulaJuridica = cedulaJuridica, Nombre = "Test Company" };
            var employees = new List<EmpleadoListDto>
            {
                new EmpleadoListDto { Cedula = "111111111" },
                new EmpleadoListDto { Cedula = "222222222" }
            };
            var benefits = new List<BeneficioModel>
            {
                new BeneficioModel { IdBeneficio = 1, Nombre = "Benefit 1" }
            };

            _empresaRepositoryMock.Setup(x => x.GetByCedulaJuridica(cedulaJuridica)).Returns(empresa);
            _empresaRepositoryMock.Setup(x => x.CheckCompanyPayroll(empresa)).Returns(0);
            _empleadoRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(employees);
            _beneficioRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(benefits);
            _employeeDeletionRepositoryMock.Setup(x => x.ExecutePhysicalDeleteAsync("111111111")).ReturnsAsync(true);
            _employeeDeletionRepositoryMock.Setup(x => x.ExecutePhysicalDeleteAsync("222222222")).ReturnsAsync(false); // fail
            _beneficioRepositoryMock.Setup(x => x.PhysicalDeletion(It.IsAny<int>()));
            _empresaRepositoryMock.Setup(x => x.DeleteEmpresa(cedulaJuridica, false)).Returns(Task.CompletedTask);

            var result = await _service.DeleteCompany(cedulaJuridica);

            Assert.False(result.Success);
            Assert.Equal(2, result.EmployeesProcessed);
            Assert.Equal(1, result.SuccessfulDeletions);
            Assert.Equal(1, result.BenefitsProcessed);
            Assert.Equal(1, result.SuccessfulBenefitDeletions);
        }

        [Fact]
        public async Task DeleteCompany_WhenNoEmployeesOrBenefits_ReturnsSuccess()
        {
            var cedulaJuridica = 123456789L;
            var empresa = new EmpresaModel { CedulaJuridica = cedulaJuridica, Nombre = "Test Company" };

            _empresaRepositoryMock.Setup(x => x.GetByCedulaJuridica(cedulaJuridica)).Returns(empresa);
            _empresaRepositoryMock.Setup(x => x.CheckCompanyPayroll(empresa)).Returns(0);
            _empleadoRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(new List<EmpleadoListDto>());
            _beneficioRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(new List<BeneficioModel>());
            _empresaRepositoryMock.Setup(x => x.DeleteEmpresa(cedulaJuridica, false)).Returns(Task.CompletedTask);

            var result = await _service.DeleteCompany(cedulaJuridica);

            Assert.True(result.Success);
            Assert.Equal(0, result.EmployeesProcessed);
            Assert.Equal(0, result.SuccessfulDeletions);
            Assert.Equal(0, result.BenefitsProcessed);
            Assert.Equal(0, result.SuccessfulBenefitDeletions);
        }

        [Fact]
        public async Task DeleteCompany_WhenExceptionOccurs_ReturnsErrorResult()
        {
            var cedulaJuridica = 123456789L;
            _empresaRepositoryMock.Setup(x => x.GetByCedulaJuridica(cedulaJuridica)).Throws(new Exception("Database connection failed"));

            var result = await _service.DeleteCompany(cedulaJuridica);

            Assert.False(result.Success);
            Assert.Contains("Error interno", result.Message);
            Assert.Contains("Database connection failed", result.Message);
            Assert.Equal(0, result.EmployeesProcessed);
            Assert.Equal(0, result.SuccessfulDeletions);
            Assert.Equal(0, result.BenefitsProcessed);
            Assert.Equal(0, result.SuccessfulBenefitDeletions);
        }

        [Fact]
        public async Task DeleteCompany_WhenBenefitHasDeductions_SetsLastPeriodInLogicalDeletion()
        {
            var cedulaJuridica = 123456789L;
            var empresa = new EmpresaModel { CedulaJuridica = cedulaJuridica, Nombre = "Test Company" };
            var benefits = new List<BeneficioModel>
            {
                new BeneficioModel { IdBeneficio = 1, Nombre = "Benefit with deductions" }
            };

            _empresaRepositoryMock.Setup(x => x.GetByCedulaJuridica(cedulaJuridica)).Returns(empresa);
            _empresaRepositoryMock.Setup(x => x.CheckCompanyPayroll(empresa)).Returns(1);
            _empleadoRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(new List<EmpleadoListDto>());
            _beneficioRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(benefits);
            _beneficioRepositoryMock.Setup(x => x.ExistsInEmployerBenefitDeductions(1)).Returns(true);
            _beneficioRepositoryMock.Setup(x => x.LogicalDeletion(It.IsAny<int>(), It.IsAny<DateTime?>()));
            _empresaRepositoryMock.Setup(x => x.DeleteEmpresa(cedulaJuridica, true)).Returns(Task.CompletedTask);

            var result = await _service.DeleteCompany(cedulaJuridica);

            Assert.True(result.Success);
            // verifica que logical deletion se llamó con una fecha (no null)
            _beneficioRepositoryMock.Verify(x => x.LogicalDeletion(1, It.Is<DateTime?>(d => d.HasValue)), Times.Once);
        }

        [Fact]
        public async Task DeleteCompany_WhenEmployeeDeletionThrowsException_ContinuesProcessingOthers()
        {
            var cedulaJuridica = 123456789L;
            var empresa = new EmpresaModel { CedulaJuridica = cedulaJuridica, Nombre = "Test Company" };
            var employees = new List<EmpleadoListDto>
            {
                new EmpleadoListDto { Cedula = "111111111" },
                new EmpleadoListDto { Cedula = "222222222" },
                new EmpleadoListDto { Cedula = "333333333" }
            };

            _empresaRepositoryMock.Setup(x => x.GetByCedulaJuridica(cedulaJuridica)).Returns(empresa);
            _empresaRepositoryMock.Setup(x => x.CheckCompanyPayroll(empresa)).Returns(0);
            _empleadoRepositoryMock.Setup(x => x.GetByEmpresa(cedulaJuridica)).Returns(employees);
            _beneficio_repository_setup_for_empty();

            _employeeDeletionRepositoryMock.Setup(x => x.ExecutePhysicalDeleteAsync("111111111")).ReturnsAsync(true);
            _employeeDeletionRepositoryMock.Setup(x => x.ExecutePhysicalDeleteAsync("222222222")).ThrowsAsync(new Exception("Deletion error"));
            _employeeDeletionRepositoryMock.Setup(x => x.ExecutePhysicalDeleteAsync("333333333")).ReturnsAsync(true);

            _empresaRepositoryMock.Setup(x => x.DeleteEmpresa(cedulaJuridica, false)).Returns(Task.CompletedTask);

            var result = await _service.DeleteCompany(cedulaJuridica);

            Assert.False(result.Success);
            Assert.Equal(3, result.EmployeesProcessed);
            Assert.Equal(2, result.SuccessfulDeletions);

            _employeeDeletionRepositoryMock.Verify(x => x.ExecutePhysicalDeleteAsync("111111111"), Times.Once);
            _employeeDeletionRepositoryMock.Verify(x => x.ExecutePhysicalDeleteAsync("222222222"), Times.Once);
            _employeeDeletionRepositoryMock.Verify(x => x.ExecutePhysicalDeleteAsync("333333333"), Times.Once);
        }

        private void _beneficio_repository_setup_for_empty()
        {
            _beneficioRepositoryMock.Setup(x => x.GetByEmpresa(It.IsAny<long>())).Returns(new List<BeneficioModel>());
        }
    }
}
