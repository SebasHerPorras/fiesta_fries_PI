using backend.Models;
using backend.Services;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmpleadoController : ControllerBase
    {
        private readonly EmpleadoService empleadoService;
        private readonly ILogger<EmpleadoController> _logger;

        public EmpleadoController(ILogger<EmpleadoController> logger)
        {
            empleadoService = new EmpleadoService();
            _logger = logger;
        }

        [HttpPost("create-with-person")]
        public ActionResult<EmpleadoModel> CreateWithPerson([FromBody] EmpleadoCreateRequest req)
        {
            if (req == null)
                return BadRequest("Request nulo.");

            if (req.personaId <= 0)
                return BadRequest("El campo personaId es requerido y debe ser mayor que 0.");

            var empleado = empleadoService.CreateEmpleadoWithPersonaAndUser(req);
            return Ok(empleado);
        }

        [HttpGet("empresa/{cedulaJuridica}")]
        public ActionResult<List<EmpleadoListDto>> GetByEmpresa(long cedulaJuridica)
        {
            var empleados = empleadoService.GetByEmpresa(cedulaJuridica);
            return Ok(empleados);
        }

        [HttpPost("EmailNotificationMessage")]
        public ActionResult notyfyEmployer(UserModel request)
        {
            EmailEmployeeNotificationService service = new EmailEmployeeNotificationService();
            service.buildEmail(request);
            return Ok();
        }

        [HttpGet("GetEmployeeHireDate")]
        public ActionResult getHireDate([FromQuery] int id)
        {
            var hireDate = this.empleadoService.GetHireDate(id);
            return Ok(hireDate);
        }

        [HttpGet("GetEmployeeWorkDayHours")]
        public ActionResult getWorkDayHours([FromQuery] DateTime dateW, DateTime dateD, int id)
        {
            EmployeeWorkDayHoursService service = new EmployeeWorkDayHoursService();
            EmployeeWorkDayModel? workHours = service.GetWorkDay(dateW, dateD, id);

            if (workHours != null)
            {
                return Ok(workHours);
            }

            return BadRequest("El Día asociado a este empleado no existe");
        }

        [HttpGet("AddWorkHours")]
        public ActionResult AddWorkHours([FromQuery] DateTime dateW, DateTime dateD, int hours, int id)
        {
            EmployeeWorkDayHoursService service = new EmployeeWorkDayHoursService();
            EmployeeWorkDayModel? workHours = service.AddHours(dateW, dateD, hours, id);

            if (workHours != null)
            {
                return Ok(workHours);
            }

            return BadRequest("No se pudieron añadir las horas");
        }

        [HttpGet("GetWorkHoursWeek")]
        public ActionResult getWeekHours([FromQuery] DateTime date, int id)
        {
            EmployeeWorkWeekService service = new EmployeeWorkWeekService();
            WeekEmployeeModel? workHours = service.GetWeek(date, id);

            if (workHours != null)
            {
                return Ok(workHours);
            }

            return BadRequest("No se pudieron traer las horas");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmpleadoPorId(int id)
        {
            var dto = await empleadoService.GetEmpleadoPersonaByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmpleado(int id, [FromBody] EmpleadoUpdateDto dto)
        {
            var result = await empleadoService.UpdateEmpleadoAsync(id, dto);
            if (!result)
                return StatusCode(500, "Error al actualizar");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpleado(int id, [FromQuery] long companyId)
        {
            if (id <= 0)
                return BadRequest(new { success = false, message = "ID de empleado inválido" });

            if (companyId <= 0)
                return BadRequest(new { success = false, message = "ID de empresa inválido" });

            try
            {
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                
                var deletionRepository = new EmployeeDeletionRepository(
                    loggerFactory.CreateLogger<EmployeeDeletionRepository>());
                
                var deletionService = new EmployeeDeletionService(
                    deletionRepository,
                    loggerFactory.CreateLogger<EmployeeDeletionService>());

                var result = await deletionService.DeleteEmpleadoAsync(id, companyId);

                if (!result.Success)
                    return BadRequest(new { success = false, message = result.Message });

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    deletionType = result.DeletionType.ToString().ToLower(),
                    wasPhysicalDelete = result.DeletionType == DeletionType.Physical,
                    wasLogicalDelete = result.DeletionType == DeletionType.Logical,
                    payrollInfo = new
                    {
                        hasPayments = result.PayrollStatus.HasPayments,
                        paymentCount = result.PayrollStatus.PaymentCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando empleado {EmployeeId}", id);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al eliminar empleado",
                    error = ex.Message
                });
            }
        }
    }
}