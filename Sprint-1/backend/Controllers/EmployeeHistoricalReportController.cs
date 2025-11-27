using backend.Interfaces;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeHistoricalReportController : ControllerBase
    {
        private readonly IEmployeeHistoricalReportService _reportService;
        private readonly EmployeeHistoricalReportCsvService _csvService;
        private readonly ILogger<EmployeeHistoricalReportController> _logger;

        public EmployeeHistoricalReportController(
            IEmployeeHistoricalReportService reportService,
            EmployeeHistoricalReportCsvService csvService,
            ILogger<EmployeeHistoricalReportController> logger)
        {
            _reportService = reportService;
            _csvService = csvService;
            _logger = logger;
        }

      
        [HttpGet]
        public async Task<IActionResult> GetReport(
            [FromQuery] long employeeId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService
                    .GenerateReportAsync(employeeId, startDate, endDate);

                return Ok(new { success = true, data = report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Employee Historical Report");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

       
        [HttpGet("csv")]
        [Produces("text/csv")]
        public async Task<IActionResult> GenerateCsv(
            [FromQuery] long employeeId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService
                    .GenerateReportAsync(employeeId, startDate, endDate);

                var csvBytes = await _csvService.GenerateCsvAsync(report);

                var fileName = $"Historico_Empleado_{employeeId}_{DateTime.Now:yyyy-MM-dd}.csv";

                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Employee Historical Report CSV");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
