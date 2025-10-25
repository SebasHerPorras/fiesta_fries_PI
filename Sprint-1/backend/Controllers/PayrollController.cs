using Microsoft.AspNetCore.Mvc;
using backend.Models.Payroll.Requests;
using backend.Interfaces.Services;
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
    }
}