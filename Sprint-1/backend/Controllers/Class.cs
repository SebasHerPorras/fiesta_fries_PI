using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PruebasCargasSocialesController : ControllerBase
    {
        private readonly CalculatorDeductionsEmployerService _calculadoraService;
        private readonly EmployerSocialSecurityContributionsService _cargasSocialesService;

        public PruebasCargasSocialesController()
        {
            _calculadoraService = new CalculatorDeductionsEmployerService();
            _cargasSocialesService = new EmployerSocialSecurityContributionsService();
        }

        [HttpPost("calcular-deducciones")]
        public ActionResult<ResultadoDeduccionesEmpleadorDto> CalcularDeducciones([FromBody] EmployeeCalculationDto empleado)
        {
            try
            {
                var cargasSociales = _cargasSocialesService.GetActiveContributions();

                if (cargasSociales == null || !cargasSociales.Any())
                {
                    return BadRequest(new
                    {
                        error = "No se encontraron cargas sociales activas en la base de datos",
                        mensaje = "Verifique que existan registros activos en la tabla EmployerSocialSecurityContributions"
                    });
                }

                var resultado = _calculadoraService.CalcularDeduccionesEmpleador(empleado, cargasSociales);

                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    error = "Error de validación",
                    detalle = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    mensaje = "Consulte los logs para más detalles"
                });
            }
        }

        [HttpGet("cargas-sociales")]
        public ActionResult<List<EmployerSocialSecurityContributions>> ObtenerCargasSociales()
        {
            try
            {
                var cargasSociales = _cargasSocialesService.GetActiveContributions();
                return Ok(cargasSociales);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al consultar cargas sociales" });
            }
        }
    }
}
