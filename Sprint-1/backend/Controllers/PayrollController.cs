using backend.Interfaces.Services;
using backend.Models.Payroll;
using backend.Models.Payroll.Requests;
using backend.Models.Payroll.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayrollController : ControllerBase
    {
        private readonly IPayrollProcessingService _payrollProcessingService;
        private readonly ILogger<PayrollController> _logger;

        public PayrollController(
            IPayrollProcessingService payrollProcessingService,
            ILogger<PayrollController> logger)
        {
            _payrollProcessingService = payrollProcessingService ?? throw new ArgumentNullException(nameof(payrollProcessingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("company/{companyId}")]
        [ProducesResponseType(typeof(List<PayrollSummaryResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPayrollsByCompany(string companyId)
        {
            _logger.LogInformation("Obteniendo planillas para compañía: {CompanyId}", companyId);

            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest(new { error = "CompanyId es requerido" });
            }

            try
            {
                var payrolls = await _payrollProcessingService.GetPayrollsByCompanyAsync(companyId);

                if (payrolls == null || !payrolls.Any())
                {
                    _logger.LogInformation("No se encontraron planillas para la compañía: {CompanyId}", companyId);
                    return Ok(new List<PayrollSummaryResult>()); 
                }

                _logger.LogInformation("Se encontraron {Count} planillas para la compañía: {CompanyId}", payrolls.Count, companyId);
                return Ok(payrolls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo planillas para compañía: {CompanyId}", companyId);
                return StatusCode(500, new { error = "Error interno del servidor al obtener planillas" });
            }
        }

        [HttpGet("{companyId}/next-period")]
        [ProducesResponseType(typeof(PayrollPeriod), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PayrollPeriod>> GetNextPeriod(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest(new { error = "CompanyId es requerido" });
            }

            try
            {
                return await _payrollProcessingService.GetNextPayrollPeriodAsync(companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo próximo periodo para compañía: {CompanyId}", companyId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("{companyId}/overdue-periods")]
        [ProducesResponseType(typeof(List<PayrollPeriod>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PayrollPeriod>>> GetOverduePeriods(string companyId)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest(new { error = "CompanyId es requerido" });
            }

            try
            {
                return await _payrollProcessingService.GetOverduePeriodsAsync(companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo periodos atrasados para compañía: {CompanyId}", companyId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("{companyId}/pending-periods")]
        [ProducesResponseType(typeof(List<PayrollPeriod>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<PayrollPeriod>>> GetPendingPeriods(string companyId, [FromQuery] int months = 6)
        {
            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest(new { error = "CompanyId es requerido" });
            }

            try
            {
                return await _payrollProcessingService.GetPendingPeriodsAsync(companyId, months);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo periodos pendientes para compañía: {CompanyId}", companyId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpPost("process")]
        [ProducesResponseType(typeof(PayrollProcessResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PayrollProcessResult), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayroll([FromBody] PayrollProcessRequest request)
        {
            _logger.LogInformation(
                "Solicitud de procesamiento de nómina - Compañía: {CompanyId}, Período: {PeriodDate}",
                request.CompanyId, request.PeriodDate.ToString("yyyy-MM"));

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Solicitud inválida. Errores: {ModelErrors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _payrollProcessingService.ProcessPayrollAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Procesamiento exitoso - Planilla ID: {PayrollId}, Empleados: {EmployeeCount}",
                        result.PayrollId, result.ProcessedEmployees);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning(
                        "Procesamiento fallido - Razón: {ErrorMessage}",
                        result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error inesperado procesando nómina - Compañía: {CompanyId}",
                    request.CompanyId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpPost("preview")]
        [ProducesResponseType(typeof(PayrollProcessResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PreviewPayroll([FromBody] PayrollPreviewRequest request)
        {
            _logger.LogInformation(
                "Solicitud de PREVIEW de nómina - Compañía: {CompanyId}, Período: {PeriodDate}",
                request.CompanyId, request.PeriodDate.ToString("yyyy-MM-dd"));

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _payrollProcessingService.PreviewPayrollAsync(request);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Preview exitoso - Empleados: {EmployeeCount}",
                        result.ProcessedEmployees);
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error en preview de nómina - Compañía: {CompanyId}",
                    request.CompanyId);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        public class PayrollPreviewRequest
        {
            public long CompanyId { get; set; }
            public DateTime PeriodDate { get; set; }
        }
    }
}