using backend.Interfaces;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployerHistoricalReportController : ControllerBase
    {
        private readonly IEmployerHistoricalReportService _reportService;
        private readonly EmployerHistoricalReportCsvService _csvService;
        private readonly ILogger<EmployerHistoricalReportController> _logger;

        public EmployerHistoricalReportController(
            IEmployerHistoricalReportService reportService,
            EmployerHistoricalReportCsvService csvService,
            ILogger<EmployerHistoricalReportController> logger)
        {
            _reportService = reportService;
            _csvService = csvService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] long employerId, [FromQuery] long? companyId,
            [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService.GenerateReportAsync(employerId, companyId,
                    startDate, endDate);
                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Employer Historical Report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("csv")]
        [Produces("text/csv")]
        public async Task<IActionResult> GenerateCsv([FromQuery] long employerId, [FromQuery] long? companyId,
            [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService.GenerateReportAsync(employerId, companyId,
                    startDate, endDate);
                var csvBytes = await _csvService.GenerateCsvAsync(report);

                var fileName = $"Historico_Planilla_{companyId ?? 0}_{DateTime.Now:yyyy-MM-dd}.csv";
                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Employer Historical Report CSV");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
