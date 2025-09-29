using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadoController : ControllerBase
    {
        private readonly EmpleadoService empleadoService;

        public EmpleadoController()
        {
            empleadoService = new EmpleadoService();
        }

        [HttpPost("create-with-person")]
        public ActionResult<EmpleadoModel> CreateWithPerson([FromBody] EmpleadoCreateRequest req)
        {
            if (req == null)
                return BadRequest("Request nulo.");

            if (req.personaId <= 0)
                return BadRequest("El campo personaId es requerido y debe ser mayor que 0.");

            // Validaciones m�nimas: email/password si la persona no existe se usar�n para crear el user
            var empleado = empleadoService.CreateEmpleadoWithPersonaAndUser(req);

            // Retorna el empleado creado (puedes retornar solo el id si prefieres)
            return Ok(empleado);
        }
    }
}