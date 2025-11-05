using backend.Models;
using backend.Services;
using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PruebasDeduccionesEmpleadoController : ControllerBase
    {
        private readonly ICalculatorDeductionsEmployeeService _calculadoraService;

        public PruebasDeduccionesEmpleadoController(ICalculatorDeductionsEmployeeService calculadoraService)
        {
            _calculadoraService = calculadoraService;
        }

        [HttpPost("calcular-deducciones-empleado")]
        public ActionResult<decimal> CalcularDeduccionesEmpleado([FromBody] CalcularDeduccionesEmpleadoRequest request)
        {
            try
            {
                var totalDeductions = _calculadoraService.CalculateEmployeeDeductions(
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

        [HttpGet("deducciones-sociales-empleado")]
        public ActionResult<List<EmployeeSocialSecurityContributions>> ObtenerDeduccionesSociales()
        {
            try
            {
                var deduccionesSociales = _calculadoraService.ObtenerDeduccionesSocialesActuales();
                return Ok(deduccionesSociales);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Error al consultar deducciones sociales" });
            }
        }

        [HttpGet("escalas-impuesto-renta")]
        public ActionResult<List<PersonalIncomeTax>> ObtenerEscalasImpuesto()
        {
            try
            {
                var escalasImpuesto = _calculadoraService.ObtenerEscalasImpuestoRenta();
                return Ok(escalasImpuesto);
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Error al consultar escalas de impuesto" });
            }
        }
    }

    public class CalcularDeduccionesEmpleadoRequest
    {
        public required EmployeeCalculationDto Empleado { get; set; }
        public int IdReporte { get; set; }
        public long CedulaJuridicaEmpresa { get; set; }
    }
}