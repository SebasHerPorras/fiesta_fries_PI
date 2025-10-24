using backend.Models;
using backend.Services;
using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PruebasCargasSocialesController : ControllerBase
    {
        private readonly ICalculatorDeductionsEmployerService _calculadoraService;

        public PruebasCargasSocialesController(ICalculatorDeductionsEmployerService calculadoraService)
        {
            _calculadoraService = calculadoraService;
        }

        [HttpPost("calcular-deducciones")]
        public ActionResult<decimal> CalcularDeducciones([FromBody] CalcularDeduccionesRequest request)
        {
            try
            {
                var totalDeductions = _calculadoraService.CalculateEmployerDeductions(
                    request.Empleado, 
                    request.IdReporte, 
                    request.CedulaJuridicaEmpresa);
                return Ok(totalDeductions);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "Error de validación", detalle = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("cargas-sociales")]
        public ActionResult<List<EmployerSocialSecurityContributions>> ObtenerCargasSociales()
        {
            try
            {
                var cargasSociales = _calculadoraService.ObtenerCargasSocialesActuales();
                return Ok(cargasSociales);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Error al consultar cargas sociales" });
            }
        }
    }

    public class CalcularDeduccionesRequest
    {
        public required EmployeeCalculationDto Empleado { get; set; }
        public int IdReporte { get; set; }
        public long CedulaJuridicaEmpresa { get; set; }
    }
}
