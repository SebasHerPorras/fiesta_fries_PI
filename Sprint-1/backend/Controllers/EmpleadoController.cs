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

            // Validaciones mínimas: email/password si la persona no existe se usarán para crear el user
            var empleado = empleadoService.CreateEmpleadoWithPersonaAndUser(req);

            // Retorna el empleado creado (puedes retornar solo el id si prefieres)
            return Ok(empleado);
        }

        [HttpGet("empresa/{cedulaJuridica}")]
        public ActionResult<List<EmpleadoListDto>> GetByEmpresa(long cedulaJuridica)
        {
            var empleados = empleadoService.GetByEmpresa(cedulaJuridica);
            Console.WriteLine("\n\n\n\nEmpleados obtenidos:");
            Console.Write(empleados);
            return Ok(empleados);
        }


        [HttpPost("EmailNotificationMessage")]
        public ActionResult notyfyEmployer(UserModel request)
        {
            EmailEmployeeNotificationService service = new EmailEmployeeNotificationService();

            service.sendEmail(request);

            return Ok();
        }

        [HttpGet("GetEmployeeHireDate")]
        public ActionResult getHireDate([FromQuery] int id)
        {
            var hireDate = this.empleadoService.GetHireDate(id);

            return Ok(hireDate);
        }

        [HttpGet("GetEmployeeWorkDayHours")]
        public ActionResult getWorkDayHours([FromQuery] DateTime dateW, DateTime dateD,int id)
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
        public ActionResult AddWorkHours([FromQuery] DateTime dateW,DateTime dateD, int hours, int id)
        {
            EmployeeWorkDayHoursService service = new EmployeeWorkDayHoursService();

            EmployeeWorkDayModel? workHours = service.AddHours(dateW,dateD, hours,id);

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

    }
}