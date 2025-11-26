using backend.Interfaces;
using backend.Models;
using backend.Repositories;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class CompanyDeletionService : ICompanyDeletionService
    {
        private readonly ILogger<CompanyDeletionService> _logger;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IEmployeeDeletionRepository _employeeDeletionRepository;
        private readonly IBeneficioRepository _beneficioRepository;

        public CompanyDeletionService(
            ILogger<CompanyDeletionService> logger,
            IEmpleadoRepository empleadoRepository,
            IEmpresaRepository empresaRepository,
            IEmployeeDeletionRepository employeeDeletionRepository,
            IBeneficioRepository beneficioRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _empleadoRepository = empleadoRepository ?? throw new ArgumentNullException(nameof(empleadoRepository));
            _empresaRepository = empresaRepository ?? throw new ArgumentNullException(nameof(empresaRepository));
            _employeeDeletionRepository = employeeDeletionRepository ?? throw new ArgumentNullException(nameof(employeeDeletionRepository));
            _beneficioRepository = beneficioRepository ?? throw new ArgumentNullException(nameof(beneficioRepository));
        }

        public async Task<CompanyDeletionResult> DeleteCompany(long cedulaJuridica)
        {
            _logger.LogInformation("Iniciando proceso de borrado completo para empresa {CedulaJuridica}", cedulaJuridica);

            try
            {
                var empresa = await GetEmpresaAsync(cedulaJuridica);
                if (empresa == null)
                {
                 
                    return new CompanyDeletionResult
                    {
                        Success = false,
                        Message = "No se encontró la empresa",
                        EmployeesProcessed = 0,
                        SuccessfulDeletions = 0,
                        BenefitsProcessed = 0,
                        SuccessfulBenefitDeletions = 0
                    };
                }

                var hasPayroll = await CheckPayrollStatusAsync(empresa);

                var employeesTask = ProcessEmployeesAsync(cedulaJuridica, hasPayroll);
                var benefitsTask = ProcessBenefitsAsync(cedulaJuridica, hasPayroll);

                await Task.WhenAll(employeesTask, benefitsTask);
                var companyTasks = _empresaRepository.DeleteEmpresa(cedulaJuridica, hasPayroll);

                var employeeResult = await employeesTask;
                var benefitResult = await benefitsTask;
                var overallSuccess = employeeResult.Success && benefitResult.Success;
                var message = $"Empleados: {employeeResult.Message} | Beneficios: {benefitResult.Message}";

                _logger.LogInformation(
                    "Proceso de borrado completado para empresa {CedulaJuridica}. " +
                    "Empleados: {EmpSuccess}/{EmpTotal}, Beneficios: {BenSuccess}/{BenTotal}",
                    cedulaJuridica,
                    employeeResult.SuccessfulDeletions, employeeResult.EmployeesProcessed,
                    benefitResult.SuccessfulDeletions, benefitResult.BenefitsProcessed);

                return new CompanyDeletionResult
                {
                    Success = overallSuccess,
                    Message = message,
                    EmployeesProcessed = employeeResult.EmployeesProcessed,
                    SuccessfulDeletions = employeeResult.SuccessfulDeletions,
                    BenefitsProcessed = benefitResult.BenefitsProcessed,
                    SuccessfulBenefitDeletions = benefitResult.SuccessfulDeletions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el proceso de borrado completo para empresa {CedulaJuridica}", cedulaJuridica);
                return new CompanyDeletionResult
                {
                    Success = false,
                    Message = $"Error interno: {ex.Message}",
                    EmployeesProcessed = 0,
                    SuccessfulDeletions = 0,
                    BenefitsProcessed = 0,
                    SuccessfulBenefitDeletions = 0
                };
            }
        }

        private async Task<CompanyDeletionResult> ProcessEmployeesAsync(long cedulaJuridica, bool hasPayroll)
        {
            try
            {
                var empleados = await GetCompanyEmployeesAsync(cedulaJuridica);

                if (!empleados.Any())
                {
                    _logger.LogInformation("No se encontraron empleados para la empresa {CedulaJuridica}", cedulaJuridica);
                    return new CompanyDeletionResult
                    {
                        Success = true,
                        Message = "No hay empleados que borrar",
                        EmployeesProcessed = 0,
                        SuccessfulDeletions = 0,
                        BenefitsProcessed = 0,
                        SuccessfulBenefitDeletions = 0
                    };
                }

                if (!hasPayroll)
                {
                    return await DeleteEmployeesPhysicalAsync(empleados, cedulaJuridica);
                }
                else
                {
                    return await DeleteEmployeesLogicalAsync(empleados, cedulaJuridica);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando empleados para empresa {CedulaJuridica}", cedulaJuridica);
                return new CompanyDeletionResult
                {
                    Success = false,
                    Message = $"Error procesando empleados: {ex.Message}",
                    EmployeesProcessed = 0,
                    SuccessfulDeletions = 0,
                    BenefitsProcessed = 0,
                    SuccessfulBenefitDeletions = 0
                };
            }
        }

        private async Task<CompanyDeletionResult> ProcessBenefitsAsync(long cedulaJuridica, bool hasPayroll)
        {
            try
            {
                var beneficios = await GetCompanyBenefitsAsync(cedulaJuridica);

                if (!beneficios.Any())
                {
                    _logger.LogInformation("No se encontraron beneficios para la empresa {CedulaJuridica}", cedulaJuridica);
                    return new CompanyDeletionResult
                    {
                        Success = true,
                        Message = "No hay beneficios que borrar",
                        EmployeesProcessed = 0,
                        SuccessfulDeletions = 0,
                        BenefitsProcessed = 0,
                        SuccessfulBenefitDeletions = 0
                    };
                }

                if (!hasPayroll)
                {
                    return await DeleteBenefitsPhysicalAsync(beneficios, cedulaJuridica);
                }
                else
                {
                    return await DeleteBenefitsLogicalAsync(beneficios, cedulaJuridica);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando beneficios para empresa {CedulaJuridica}", cedulaJuridica);
                return new CompanyDeletionResult
                {
                    Success = false,
                    Message = $"Error procesando beneficios: {ex.Message}",
                    EmployeesProcessed = 0,
                    SuccessfulDeletions = 0,
                    BenefitsProcessed = 0,
                    SuccessfulBenefitDeletions = 0
                };
            }
        }

        private async Task<EmpresaModel> GetEmpresaAsync(long cedulaJuridica)
        {
            try
            {
                var empresa = _empresaRepository.GetByCedulaJuridica(cedulaJuridica);

                if (empresa != null)
                {
                    _logger.LogDebug("Empresa encontrada: {Nombre} ({CedulaJuridica})",
                        empresa.Nombre, empresa.CedulaJuridica);
                }
                else
                {
                    _logger.LogWarning("Empresa con cédula jurídica {CedulaJuridica} no encontrada", cedulaJuridica);
                }

                return empresa;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo empresa con cédula jurídica {CedulaJuridica}", cedulaJuridica);
                throw;
            }
        }

        private async Task<bool> CheckPayrollStatusAsync(EmpresaModel empresa)
        {
            try
            {
                var payrollResult = _empresaRepository.CheckCompanyPayroll(empresa);

                _logger.LogDebug("Resultado de verificación de payroll para {CedulaJuridica}: {Result}",
                    empresa.CedulaJuridica, payrollResult);

                return payrollResult == 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando payroll para empresa {CedulaJuridica}", empresa.CedulaJuridica);
                throw;
            }
        }

        private async Task<List<EmpleadoListDto>> GetCompanyEmployeesAsync(long cedulaJuridica)
        {
            try
            {
                var empleados = _empleadoRepository.GetByEmpresa(cedulaJuridica);

                _logger.LogInformation("Encontrados {Count} empleados para la empresa {CedulaJuridica}",
                    empleados.Count, cedulaJuridica);

                return empleados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo empleados para empresa {CedulaJuridica}", cedulaJuridica);
                throw;
            }
        }

        private async Task<List<BeneficioModel>> GetCompanyBenefitsAsync(long cedulaJuridica)
        {
            try
            {
                var beneficios = _beneficioRepository.GetByEmpresa(cedulaJuridica);

                _logger.LogInformation("Encontrados {Count} beneficios para la empresa {CedulaJuridica}",
                    beneficios.Count, cedulaJuridica);

                return beneficios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo beneficios para empresa {CedulaJuridica}", cedulaJuridica);
                throw;
            }
        }

        private async Task<CompanyDeletionResult> DeleteEmployeesLogicalAsync(List<EmpleadoListDto> empleados, long cedulaJuridica)
        {
            var deletionTasks = empleados.Select(async empleado =>
            {
                try
                {
                    _logger.LogDebug("Iniciando borrado lógico para empleado {Cedula} de empresa {CedulaJuridica}",
                        empleado.Cedula, cedulaJuridica);

                    var result = await _employeeDeletionRepository.ExecuteLogicalDeleteAsync(empleado.Cedula);

                    if (result)
                    {
                        _logger.LogInformation("Borrado lógico exitoso para empleado {Cedula}", empleado.Cedula);
                    }
                    else
                    {
                        _logger.LogWarning("Borrado lógico falló para empleado {Cedula}", empleado.Cedula);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante borrado lógico para empleado {Cedula}", empleado.Cedula);
                    return false;
                }
            }).ToList();

            var results = await Task.WhenAll(deletionTasks);
            var successfulDeletions = results.Count(result => result);

            _logger.LogInformation("Proceso de borrado lógico completado para empresa {CedulaJuridica}: {Successful}/{Total} empleados procesados exitosamente",
                cedulaJuridica, successfulDeletions, empleados.Count);

            return new CompanyDeletionResult
            {
                Success = successfulDeletions == empleados.Count,
                Message = successfulDeletions == empleados.Count
                    ? $"Todos los {empleados.Count} empleados fueron borrados lógicamente exitosamente"
                    : $"{successfulDeletions} de {empleados.Count} empleados fueron borrados lógicamente exitosamente",
                EmployeesProcessed = empleados.Count,
                SuccessfulDeletions = successfulDeletions,
                BenefitsProcessed = 0,
                SuccessfulBenefitDeletions = 0
            };
        }

        private async Task<CompanyDeletionResult> DeleteEmployeesPhysicalAsync(List<EmpleadoListDto> empleados, long cedulaJuridica)
        {
            var deletionTasks = empleados.Select(async empleado =>
            {
                try
                {
                    _logger.LogDebug("Iniciando borrado físico para empleado {Cedula} de empresa {CedulaJuridica}",
                        empleado.Cedula, cedulaJuridica);

                    var result = await _employeeDeletionRepository.ExecutePhysicalDeleteAsync(empleado.Cedula);

                    if (result)
                    {
                        _logger.LogInformation("Borrado físico exitoso para empleado {Cedula}", empleado.Cedula);
                    }
                    else
                    {
                        _logger.LogWarning("Borrado físico falló para empleado {Cedula}", empleado.Cedula);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante borrado físico para empleado {Cedula}", empleado.Cedula);
                    return false;
                }
            }).ToList();

            var results = await Task.WhenAll(deletionTasks);
            var successfulDeletions = results.Count(result => result);

            _logger.LogInformation("Proceso de borrado físico completado para empresa {CedulaJuridica}: {Successful}/{Total} empleados procesados exitosamente",
                cedulaJuridica, successfulDeletions, empleados.Count);

            return new CompanyDeletionResult
            {
                Success = successfulDeletions == empleados.Count,
                Message = successfulDeletions == empleados.Count
                    ? $"Todos los {empleados.Count} empleados fueron borrados físicamente exitosamente"
                    : $"{successfulDeletions} de {empleados.Count} empleados fueron borrados físicamente exitosamente",
                EmployeesProcessed = empleados.Count,
                SuccessfulDeletions = successfulDeletions,
                BenefitsProcessed = 0,
                SuccessfulBenefitDeletions = 0
            };
        }

        private async Task<CompanyDeletionResult> DeleteBenefitsLogicalAsync(List<BeneficioModel> beneficios, long cedulaJuridica)
        {
            var deletionTasks = beneficios.Select(async beneficio =>
            {
                try
                {
                    _logger.LogDebug("Iniciando borrado lógico para beneficio {IdBeneficio} - {Nombre} de empresa {CedulaJuridica}",
                        beneficio.IdBeneficio, beneficio.Nombre, cedulaJuridica);

                    var isUsedInDeductions = _beneficioRepository.ExistsInEmployerBenefitDeductions(beneficio.IdBeneficio);

                    DateTime? lastPeriod = isUsedInDeductions ? DateTime.Now : null;

                    _beneficioRepository.LogicalDeletion(beneficio.IdBeneficio, lastPeriod);

                    _logger.LogInformation("Borrado lógico exitoso para beneficio {IdBeneficio} - {Nombre}",
                        beneficio.IdBeneficio, beneficio.Nombre);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante borrado lógico para beneficio {IdBeneficio} - {Nombre}",
                        beneficio.IdBeneficio, beneficio.Nombre);
                    return false;
                }
            }).ToList();

            var results = await Task.WhenAll(deletionTasks);
            var successfulDeletions = results.Count(result => result);

            _logger.LogInformation("Proceso de borrado lógico completado para empresa {CedulaJuridica}: {Successful}/{Total} beneficios procesados exitosamente",
                cedulaJuridica, successfulDeletions, beneficios.Count);

            return new CompanyDeletionResult
            {
                Success = successfulDeletions == beneficios.Count,
                Message = successfulDeletions == beneficios.Count
                    ? $"Todos los {beneficios.Count} beneficios fueron borrados lógicamente exitosamente"
                    : $"{successfulDeletions} de {beneficios.Count} beneficios fueron borrados lógicamente exitosamente",
                EmployeesProcessed = 0,
                SuccessfulDeletions = 0,
                BenefitsProcessed = beneficios.Count,
                SuccessfulBenefitDeletions = successfulDeletions
            };
        }

        private async Task<CompanyDeletionResult> DeleteBenefitsPhysicalAsync(List<BeneficioModel> beneficios, long cedulaJuridica)
        {
            var deletionTasks = beneficios.Select(async beneficio =>
            {
                try
                {
                    _logger.LogDebug("Iniciando borrado físico para beneficio {IdBeneficio} - {Nombre} de empresa {CedulaJuridica}",
                        beneficio.IdBeneficio, beneficio.Nombre, cedulaJuridica);

                    _beneficioRepository.PhysicalDeletion(beneficio.IdBeneficio);

                    _logger.LogInformation("Borrado físico exitoso para beneficio {IdBeneficio} - {Nombre}",
                        beneficio.IdBeneficio, beneficio.Nombre);

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error durante borrado físico para beneficio {IdBeneficio} - {Nombre}",
                        beneficio.IdBeneficio, beneficio.Nombre);
                    return false;
                }
            }).ToList();

            var results = await Task.WhenAll(deletionTasks);
            var successfulDeletions = results.Count(result => result);


            _logger.LogInformation("Proceso de borrado físico completado para empresa {CedulaJuridica}: {Successful}/{Total} beneficios procesados exitosamente",
                cedulaJuridica, successfulDeletions, beneficios.Count);

            return new CompanyDeletionResult
            {
                Success = successfulDeletions == beneficios.Count,
                Message = successfulDeletions == beneficios.Count
                    ? $"Todos los {beneficios.Count} beneficios fueron borrados físicamente exitosamente"
                    : $"{successfulDeletions} de {beneficios.Count} beneficios fueron borrados físicamente exitosamente",
                EmployeesProcessed = 0,
                SuccessfulDeletions = 0,
                BenefitsProcessed = beneficios.Count,
                SuccessfulBenefitDeletions = successfulDeletions
            };
        }
    }
}