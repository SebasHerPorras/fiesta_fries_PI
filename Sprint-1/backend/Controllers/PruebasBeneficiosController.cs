using backend.Models;
using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PruebasBeneficiosController : ControllerBase
    {
        private readonly ICalculatorBenefitsService _benefitsService;

        public PruebasBeneficiosController(ICalculatorBenefitsService benefitsService)
        {
            _benefitsService = benefitsService;
        }

        [HttpPost("calcular-beneficios")]
        public async Task<ActionResult<decimal>> CalcularBeneficios([FromBody] CalcularBeneficiosRequest request)
        {
            try
            {
                var totalCost = await _benefitsService.CalculateBenefitsAsync(
                    request.Employee, 
                    request.ReportId, 
                    request.CedulaJuridicaEmpresa);
                
                return Ok(new
                {
                    employee = request.Employee,
                    totalEmployerCost = totalCost,
                    message = "Beneficios calculados exitosamente"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = "Error de validación", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", details = ex.Message });
            }
        }

        [HttpGet("lista-beneficios/{cedulaJuridicaEmpresa}")]
        public ActionResult<List<BenefitDto>> ObtenerListaBeneficios(long cedulaJuridicaEmpresa)
        {
            try
            {
                var benefits = _benefitsService.GetBenefitsList(cedulaJuridicaEmpresa);
                
                return Ok(new
                {
                    company = cedulaJuridicaEmpresa,
                    totalBenefits = benefits.Count,
                    benefits
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al obtener beneficios", details = ex.Message });
            }
        }
    }

    public class CalcularBeneficiosRequest
    {
        public required EmployeeCalculationDto Employee { get; set; }
        public int ReportId { get; set; }
        public long CedulaJuridicaEmpresa { get; set; }
    }
}