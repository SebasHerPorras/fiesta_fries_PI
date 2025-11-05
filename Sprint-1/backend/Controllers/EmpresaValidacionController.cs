using backend.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/empresa/validacion")]
    public class EmpresaValidacionController : ControllerBase
    {
        private readonly IEmployeeBenefitService _benefitService;

        public EmpresaValidacionController(IEmployeeBenefitService benefitService)
        {
            _benefitService = benefitService;
        }

        public class NoMaxRequest
        {
            public int nuevoMax { get; set; }
        }

        [HttpPut("modificacion-beneficios/{cedula}")]
        public IActionResult ValidarModificacionBeneficios(long cedula, [FromBody] int nuevoMax)
        {
          Console.WriteLine($"Validando empresa {cedula} con nuevoMax {nuevoMax}");
            var empleadosExcedidos = _benefitService.GetEmpleadosConBeneficiosExcedidos(cedula, nuevoMax);

            if (empleadosExcedidos.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "No se puede reducir NoMaxBeneficios. Algunos empleados tienen más beneficios asignados.",
                    empleados = empleadosExcedidos
                });
            }

            return Ok(new
            {
                success = true,
                message = "Validación exitosa. Se puede modificar NoMaxBeneficios."
            });
        }
    }

}
