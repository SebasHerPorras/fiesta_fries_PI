using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpresaController : ControllerBase
    {
        private readonly EmpresaService _empresaService;

        public EmpresaController()
        {
            _empresaService = new EmpresaService();
        }

        [HttpPost]
        public async Task<ActionResult> CreateEmpresa([FromBody] EmpresaModel empresa)
        {
            try
            {
                Console.WriteLine("=== SOLICITUD RECIBIDA ===");

                if (empresa == null)
                {
                    Console.WriteLine("Error: Empresa es null");
                    return BadRequest("Datos de empresa inválidos");
                }

             
                empresa.DueñoEmpresa = 117940664;

                Console.WriteLine($"DueñoEmpresa asignado: {empresa.DueñoEmpresa}");
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
                    return Ok(new { success = true, message = "Empresa creada exitosamente" });
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
                return StatusCode(500, new { success = false, message = $"Error interno del servidor: {ex.Message}" });
            }
        }

        [HttpGet]
        public List<EmpresaModel> Get()
        {
            var empresas = _empresaService.GetEmpresas();
            return empresas;
        }
    }
}