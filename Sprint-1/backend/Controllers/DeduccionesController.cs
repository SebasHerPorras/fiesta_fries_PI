using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")] // endpoint solamente para hacer pruebas, eliminar al subir a produccion
    [ApiController]
    public class DeduccionesController : ControllerBase
    {
        private readonly CalculatorDeductionsEmployerService _calculadoraService;

        public DeduccionesController()
        {
            _calculadoraService = new CalculatorDeductionsEmployerService();
        }

        [HttpPost("calcular-empleador")]
        public ActionResult<ResultadoDeduccionesEmpleadorDto> CalcularDeduccionesEmpleador([FromBody] EmployeeCalculationDto empleado)
        {
            try
            {
                var resultado = _calculadoraService.CalcularDeduccionesEmpleador(empleado);
                return Ok(resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }
}