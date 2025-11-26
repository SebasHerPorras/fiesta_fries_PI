using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyDeletionController : ControllerBase
    {
        private readonly ICompanyDeletionService _companyDeletionService;
        private readonly ILogger<CompanyDeletionController> _logger;

        public CompanyDeletionController(
            ICompanyDeletionService companyDeletionService,
            ILogger<CompanyDeletionController> logger)
        {
            _companyDeletionService = companyDeletionService;
            _logger = logger;
        }

        [HttpDelete("{cedulaJuridica}")]
        public async Task<ActionResult<CompanyDeletionResult>> DeleteCompany(long cedulaJuridica)
        {
            _logger.LogInformation("Solicitando borrado completo de empresa {CedulaJuridica}", cedulaJuridica);

            try
            {
 
                if (cedulaJuridica <= 0)
                {
                    _logger.LogWarning("Cédula jurídica inválida: {CedulaJuridica}", cedulaJuridica);
                    return BadRequest(new CompanyDeletionResult
                    {
                        Success = false,
                        Message = "La cédula jurídica debe ser un número positivo",
                        EmployeesProcessed = 0,
                        SuccessfulDeletions = 0,
                        BenefitsProcessed = 0,
                        SuccessfulBenefitDeletions = 0
                    });
                }

                var result = await _companyDeletionService.DeleteCompany(cedulaJuridica);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Borrado completado exitosamente para empresa {CedulaJuridica}. " +
                        "Empleados: {EmpSuccess}/{EmpTotal}, Beneficios: {BenSuccess}/{BenTotal}",
                        cedulaJuridica,
                        result.SuccessfulDeletions, result.EmployeesProcessed,
                        result.SuccessfulBenefitDeletions, result.BenefitsProcessed);

                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning(
                        "Borrado completado con errores para empresa {CedulaJuridica}. " +
                        "Empleados: {EmpSuccess}/{EmpTotal}, Beneficios: {BenSuccess}/{BenTotal}. " +
                        "Mensaje: {Message}",
                        cedulaJuridica,
                        result.SuccessfulDeletions, result.EmployeesProcessed,
                        result.SuccessfulBenefitDeletions, result.BenefitsProcessed,
                        result.Message);

                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado durante el borrado de empresa {CedulaJuridica}", cedulaJuridica);

                return StatusCode(StatusCodes.Status500InternalServerError, new CompanyDeletionResult
                {
                    Success = false,
                    Message = $"Error interno del servidor: {ex.Message}",
                    EmployeesProcessed = 0,
                    SuccessfulDeletions = 0,
                    BenefitsProcessed = 0,
                    SuccessfulBenefitDeletions = 0
                });
            }
        }

      
    }
}