using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Security.Claims;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresaController : ControllerBase
    {
        private readonly EmpresaService _empresaService;
        private readonly PersonService _personService;

        public EmpresaController()
        {
            _empresaService = new EmpresaService();
            _personService = new PersonService();
        }

        [HttpPost]
        public async Task<ActionResult> CreateEmpresa([FromBody] EmpresaRequest request)
        {
            try
            {
                Console.WriteLine("=== SOLICITUD CREAR EMPRESA ===");

                if (request == null || request.Empresa == null)
                {
                    Console.WriteLine("Error: Request o Empresa es null");
                    return BadRequest("Datos de empresa inválidos");
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
                    return BadRequest("Solo los usuarios tipo Empleador pueden crear empresas");
                }

                var empresa = request.Empresa;
                empresa.DueñoEmpresa = persona.id;

                Console.WriteLine($"DueñoEmpresa asignado: {empresa.DueñoEmpresa}");
                Console.WriteLine($"Nombre del dueño: {persona.firstName} {persona.secondName}");
                Console.WriteLine($"Cédula: {empresa.CedulaJuridica}");
                Console.WriteLine($"Nombre: {empresa.Nombre}");
                Console.WriteLine($"Dirección: {empresa.DireccionEspecifica}");
                Console.WriteLine($"Teléfono: {empresa.Telefono}");
                Console.WriteLine($"Max Beneficios: {empresa.NoMaxBeneficios}");
                Console.WriteLine($"Frecuencia Pago: {empresa.FrecuenciaPago}");
                Console.WriteLine($"Día Pago: {empresa.DiaPago}");

                var result = _empresaService.CreateEmpresa(empresa);

                if (string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("Empresa creada exitosamente");
                    return Ok(new
                    {
                        success = true,
                        message = "Empresa creada exitosamente",
                        empresaId = empresa.DueñoEmpresa
                    });
                }
                else
                {
                    Console.WriteLine($"Error al crear empresa: {result}");
                    return BadRequest(new { success = false, message = result });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al crear empresa: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public List<EmpresaModel> Get()
        {
            var empresas = _empresaService.GetEmpresas();
            return empresas;
        }

        [HttpGet("mis-empresas/{userId}")]
        public IActionResult GetEmpresasByUser(string userId)
        {
            try
            {
                Console.WriteLine("=== SOLICITUD OBTENER EMPRESAS DEL USUARIO ===");

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { success = false, message = "UserId es requerido" });
                }

                if (!Guid.TryParse(userId, out Guid userGuid))
                {
                    return BadRequest(new { success = false, message = "UserId inválido" });
                }

                Console.WriteLine($"UserId recibido: {userId}");

                var persona = _personService.GetByUserId(userGuid);
                if (persona == null)
                {
                    Console.WriteLine("Error: No se encontró persona asociada al usuario");
                    return NotFound(new { success = false, message = "No se encontró perfil de empleador para este usuario" });
                }

                Console.WriteLine($"Persona encontrada: ID {persona.id}, Nombre: {persona.firstName} {persona.secondName}");

                var empresas = _empresaService.GetEmpresasByOwner(persona.id);

                Console.WriteLine($"Se encontraron {empresas.Count} empresas para el usuario");

                return Ok(new
                {
                    success = true,
                    empresas = empresas,
                    count = empresas.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetEmpresasByUser: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetEmpresaById(int id)
        {
            try
            {
                Console.WriteLine($"=== SOLICITUD OBTENER EMPRESA POR ID: {id} ===");

                var empresa = _empresaService.GetEmpresaById(id);
                if (empresa == null)
                {
                    Console.WriteLine($"Empresa con ID {id} no encontrada");
                    return NotFound(new { success = false, message = "Empresa no encontrada" });
                }

                Console.WriteLine($"Empresa encontrada: {empresa.Nombre}");
                return Ok(new
                {
                    success = true,
                    empresa = empresa
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetEmpresaById: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}"
                });
            }
        }




    }

   
    public class EmpresaRequest
    {
        public string UserId { get; set; }
        public EmpresaModel Empresa { get; set; }
    }
}