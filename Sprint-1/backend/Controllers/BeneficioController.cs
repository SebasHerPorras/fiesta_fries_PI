using backend.Models;
using backend.Services;
using backend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeneficioController : ControllerBase
    {
        private readonly IBeneficioService _beneficioService;
        private readonly PersonService _personService;
        private readonly EmpresaService _empresaService;

        public BeneficioController(
            IBeneficioService beneficioService,
            PersonService personService,
            EmpresaService empresaService)
        {
            _beneficioService = beneficioService;
            _personService = personService;
            _empresaService = empresaService;
        }

        [HttpPost]
        public async Task<ActionResult> CreateBeneficio([FromBody] BeneficioRequest request)
        {
            try
            {
                Console.WriteLine("- Crear Beneficio -");

                if (request == null || request.Beneficio == null)
                {
                    Console.WriteLine("Error: Request o Beneficio es null");
                    return BadRequest("Datos de beneficio inválidos");
                }

                if (string.IsNullOrEmpty(request.UserId))
                {
                    Console.WriteLine("Error: UserId es requerido");
                    return BadRequest("UserId es requerido");
                }

                if (!Guid.TryParse(request.UserId, out Guid userId))
                {
                    Console.WriteLine($"Error: UserId inválido: {request.UserId}");
                    return BadRequest("UserId inválido");
                }

                Console.WriteLine($"UserId recibido: {userId}");

                var persona = _personService.GetByUserId(userId);
                if (persona == null)
                {
                    Console.WriteLine("Error: No se encontró persona asociada al usuario");
                    return BadRequest("No se encontró perfil de empleador para este usuario");
                }

                Console.WriteLine($"Persona encontrada: ID {persona.id}, Tipo: {persona.personType}");

                if (persona.personType != "Empleador")
                {
                    Console.WriteLine($"Error: El usuario no es empleador. Tipo: {persona.personType}");
                    return BadRequest("Solo los usuarios tipo Empleador pueden crear beneficios");
                }

                var beneficio = request.Beneficio;

                Console.WriteLine($"Nombre del beneficio: {beneficio.Nombre}");
                Console.WriteLine($"Tipo: {beneficio.Tipo}");
                Console.WriteLine($"Quien Asume: {beneficio.QuienAsume}");
                Console.WriteLine($"Valor: {beneficio.Valor}");
                Console.WriteLine($"Etiqueta: {beneficio.Etiqueta}");

                var result = _beneficioService.CreateBeneficio(beneficio);

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("Beneficio creado exitosamente");
                    return Ok(new
                    {
                        success = true,
                        message = "Beneficio creado exitosamente"
                    });
                }
                else
                {
                    Console.WriteLine($"Error al crear beneficio: {result}");
                    return BadRequest(new { success = false, message = result });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al crear beneficio: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBeneficio(int id, [FromBody] BeneficioRequest request)
        {
            try
            {
                if (request?.Beneficio == null)
                    return BadRequest("Datos de beneficio inválidos");

                if (!Guid.TryParse(request.UserId, out Guid userId))
                    return BadRequest("UserId inválido");                    
                
                var persona = _personService.GetByUserId(userId);
                if (persona.personType != "Empleador")
                    return BadRequest("Solo los usuarios tipo Empleador pueden modificar beneficios");

                var beneficio = _beneficioService.GetById(id);
                if (beneficio == null)
                    return NotFound("Beneficio no encontrado");

                var empresa = _empresaService.GetEmpresaByCedula(beneficio.CedulaJuridica);
                if (empresa?.DueñoEmpresa != persona.id)
                    return BadRequest("No tienes permisos para modificar este beneficio");

                var resultado = _beneficioService.UpdateBeneficio(id, request.Beneficio);
                return string.IsNullOrEmpty(resultado)
                    ? Ok(new { success = true, message = "Beneficio actualizado correctamente" })
                    : BadRequest(new { success = false, message = resultado });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateBeneficio: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { success = false, message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpGet]
        public List<BeneficioModel> GetAll()
        {
            var beneficios = _beneficioService.GetAll();
            return beneficios;
        }

        [HttpGet("por-empresa/{cedulaJuridica}")]
        public IActionResult GetBeneficiosPorEmpresa(long cedulaJuridica)
        {
            try
            {
                Console.WriteLine($"- Get Beneficios por Empresa -");

                var beneficios = _beneficioService.GetByEmpresa(cedulaJuridica);

                Console.WriteLine($"Se encontraron {beneficios.Count} beneficios para la empresa");

                return Ok(new
                {
                    success = true,
                    beneficios = beneficios,
                    count = beneficios.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetBeneficiosPorEmpresa: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetBeneficioById(int id)
        {
            try
            {
                Console.WriteLine($"=== SOLICITUD OBTENER BENEFICIO POR ID: {id} ===");

                var beneficio = _beneficioService.GetById(id);
                if (beneficio == null)
                {
                    Console.WriteLine($"Beneficio con ID {id} no encontrado");
                    return NotFound(new { success = false, message = "Beneficio no encontrado" });
                }

                Console.WriteLine($"Beneficio encontrado: {beneficio.Nombre}");
                return Ok(new
                {
                    success = true,
                    beneficio = beneficio
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetBeneficioById: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBeneficio(int id)
        {
            try
            {
                var result = _beneficioService.DeleteBeneficio(id);

                if (result.Contains("Physical"))
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Beneficio {id} eliminado físicamente"
                    });
                }
                else if (result.Contains("Logical"))
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"Beneficio {id} eliminado lógicamente"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = result
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno al eliminar beneficio: {ex.Message}"
                });
            }
        }

    }

    public class BeneficioRequest
    {
        public string UserId { get; set; }
        public BeneficioModel Beneficio { get; set; }
    }
}