using backend.Models.Payroll;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollReportController : ControllerBase
    {
        private readonly PayrollReportRepository _repository;
        private readonly PayrollPdfService _pdfService;
        private readonly PayrollCsvService _csvService;
        private readonly ILogger<PayrollReportController> _logger;

        public PayrollReportController(
            PayrollReportRepository repository,
            PayrollPdfService pdfService,
            PayrollCsvService csvService,
            ILogger<PayrollReportController> logger)
        {
            _repository = repository;
            _pdfService = pdfService;
            _csvService = csvService;
            _logger = logger;
        }


        [HttpGet("company/{companyId}/last-12")]
        public async Task<IActionResult> GetLast12Payrolls(long companyId)
        {
            try
            {
                var payrolls = await _repository.GetLast12PayrollsAsync(companyId);
                return Ok(new { success = true, payrolls });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo últimas planillas para empresa {CompanyId}", companyId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpGet("{payrollId}")]
        public async Task<IActionResult> GetFullReport(int payrollId)
        {
            try
            {
                var report = await _repository.GetPayrollFullReportAsync(payrollId);
                return Ok(new { success = true, report });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reporte para planilla {PayrollId}", payrollId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{payrollId}/pdf")]
        [Produces("application/pdf", "application/json")] // ?? AGREGAR JSON como fallback
        public async Task<IActionResult> GeneratePdf(int payrollId)
        {
            try
            {
                var report = await _repository.GetPayrollFullReportAsync(payrollId);
                var pdfBytes = await _pdfService.GeneratePayrollPdfAsync(report);

                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return NotFound(new { success = false, message = "No se pudo generar el PDF" });
                }

                var fileName = $"Planilla_{report.Header.NombreEmpresa}_{report.Header.PeriodDate:yyyy-MM}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando PDF para planilla {PayrollId}", payrollId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpGet("{payrollId}/csv")]
        [Produces("text/csv")]
        public async Task<IActionResult> GenerateCsv(int payrollId)
        {
            try
            {
                var report = await _repository.GetPayrollFullReportAsync(payrollId);
                var csvBytes = await _csvService.GeneratePayrollCsvAsync(report);

                var fileName = $"Planilla_{report.Header.NombreEmpresa}_{report.Header.PeriodDate:yyyy-MM}.csv";

                return File(csvBytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando CSV para planilla {PayrollId}", payrollId);
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}