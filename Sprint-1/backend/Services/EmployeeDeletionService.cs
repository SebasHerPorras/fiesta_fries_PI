using backend.Interfaces;
using backend.Models;
using backend.Repositories;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class EmployeeDeletionService : IEmployeeDeletionService
    {
        private readonly EmployeeDeletionRepository _repository;
        private readonly ILogger<EmployeeDeletionService> _logger;

        public EmployeeDeletionService(
            EmployeeDeletionRepository repository,
            ILogger<EmployeeDeletionService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Método principal que orquesta la eliminación del empleado
        /// </summary>
        public async Task<EmployeeDeletionResult> DeleteEmpleadoAsync(int personaId, long companyId)
        {
            _logger.LogInformation(
                "Iniciando proceso de eliminación para empleado {PersonaId} de empresa {CompanyId}",
                personaId, companyId);

            try
            {
                // 1. Validar que el empleado existe
                if (!await ValidateEmployeeExistsAsync(personaId, companyId))
                {
                    return CreateErrorResult("Empleado no encontrado o no pertenece a esta empresa");
                }

                // 2. Validar que no es dueño de empresa
                if (await IsCompanyOwnerAsync(personaId))
                {
                    return CreateErrorResult("No se puede eliminar: el empleado es dueño de una empresa");
                }

                // 3. Verificar estado de pagos
                var payrollStatus = await CheckPayrollStatusAsync(personaId);

                // 4. Decidir tipo de borrado y ejecutar
                EmployeeDeletionResult result;

                if (payrollStatus.HasPayments)
                {
                    // BORRADO LÓGICO
                    _logger.LogInformation(
                        "Empleado {PersonaId} tiene {Count} pagos. Aplicando borrado LÓGICO",
                        personaId, payrollStatus.PaymentCount);

                    result = await ExecuteLogicalDeleteAsync(personaId, payrollStatus);
                }
                else
                {
                    // BORRADO FÍSICO
                    _logger.LogInformation(
                        "Empleado {PersonaId} sin pagos. Aplicando borrado FÍSICO",
                        personaId);

                    result = await ExecutePhysicalDeleteAsync(personaId, payrollStatus);
                }

                _logger.LogInformation(
                    "Eliminación completada exitosamente. Tipo: {Type}",
                    result.DeletionType);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error eliminando empleado {PersonaId}", personaId);
                
                return CreateErrorResult($"Error interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica el estado de pagos en planilla
        /// </summary>
        public async Task<EmployeePayrollStatus> CheckPayrollStatusAsync(int personaId)
        {
            _logger.LogDebug("Verificando estado de pagos para empleado {PersonaId}", personaId);

            return await _repository.CheckPayrollStatusAsync(personaId);
        }

        #region Private Methods - Validaciones

        /// <summary>
        /// Valida que el empleado existe y pertenece a la empresa
        /// </summary>
        private async Task<bool> ValidateEmployeeExistsAsync(int personaId, long companyId)
        {
            // TODO: Implementar validación con repository
            return await _repository.ValidateEmployeeExistsAsync(personaId, companyId);
        }

        /// <summary>
        /// Valida que el empleado no sea dueño de empresa
        /// </summary>
        private async Task<bool> IsCompanyOwnerAsync(int personaId)
        {
            // TODO: Implementar validación
            return await _repository.IsCompanyOwnerAsync(personaId);
        }

        #endregion

        #region Private Methods - Ejecución de borrado

        /// <summary>
        /// Ejecuta el borrado LÓGICO (marca IsDeleted = 1)
        /// </summary>
        private async Task<EmployeeDeletionResult> ExecuteLogicalDeleteAsync(
            int personaId, 
            EmployeePayrollStatus payrollStatus)
        {
            _logger.LogDebug("Ejecutando borrado lógico para empleado {PersonaId}", personaId);

            var success = await _repository.ExecuteLogicalDeleteAsync(personaId);

            if (!success)
            {
                return CreateErrorResult("Error al ejecutar borrado lógico en base de datos");
            }

            return new EmployeeDeletionResult
            {
                Success = true,
                Message = $"Empleado desactivado correctamente (borrado lógico). " +
                          $"Tiene {payrollStatus.PaymentCount} pago(s) registrado(s).",
                DeletionType = DeletionType.Logical,
                PayrollStatus = payrollStatus,
                DeletedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Ejecuta el borrado FÍSICO (DELETE en cascada)
        /// </summary>
        private async Task<EmployeeDeletionResult> ExecutePhysicalDeleteAsync(
            int personaId, 
            EmployeePayrollStatus payrollStatus)
        {
            _logger.LogDebug("Ejecutando borrado físico para empleado {PersonaId}", personaId);

            var success = await _repository.ExecutePhysicalDeleteAsync(personaId);

            if (!success)
            {
                return CreateErrorResult("Error al ejecutar borrado físico en base de datos");
            }

            return new EmployeeDeletionResult
            {
                Success = true,
                Message = "Empleado eliminado completamente del sistema (nunca estuvo en planilla).",
                DeletionType = DeletionType.Physical,
                PayrollStatus = payrollStatus,
                DeletedAt = DateTime.Now
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Crea un resultado de error estandarizado
        /// </summary>
        private EmployeeDeletionResult CreateErrorResult(string message)
        {
            return new EmployeeDeletionResult
            {
                Success = false,
                Message = message,
                DeletionType = DeletionType.Logical, // Default
                PayrollStatus = new EmployeePayrollStatus(),
                DeletedAt = DateTime.Now
            };
        }

        #endregion
    }
}