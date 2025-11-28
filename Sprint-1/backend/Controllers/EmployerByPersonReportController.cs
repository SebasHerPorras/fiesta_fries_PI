using backend.Interfaces;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
      [Route("api/[controller]")]
    [ApiController]
    public class EmployerByPersonReportController : ControllerBase
    {
        private readonly IEmployerByPersonReportService _reportService;
        private readonly EmployerByPersonReportCsvService _csvService;
        private readonly ILogger<EmployerByPersonReportController> _logger;

        public EmployerByPersonReportController(
            IEmployerByPersonReportService reportService,
            EmployerByPersonReportCsvService csvService,
            ILogger<EmployerByPersonReportController> logger)
        {
            _reportService = reportService;
            _csvService = csvService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetReportPerPerson([FromQuery] long employerId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
            [FromQuery] String? employmentType, [FromQuery] long? companyId, [FromQuery] int? cedula)
        {
            try
            {
                var report = await _reportService.GetReportAsync(employerId,startDate, endDate,
                    employmentType, companyId, cedula);
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
        public async Task<IActionResult> GenerateCsv([FromQuery] long employerId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
            [FromQuery] String? employmentType, [FromQuery] long? companyId, [FromQuery] int? cedula)
        {
            try
            {
                var report = await _reportService.GetReportAsync(employerId, startDate, endDate,
                    employmentType, companyId, cedula);
                var csvBytes = await _csvService.GenerateCsvAsync(report);

                var fileName = $"Planilla_Por_Persona{companyId ?? 0}_{DateTime.Now:yyyy-MM-dd}.csv";
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
