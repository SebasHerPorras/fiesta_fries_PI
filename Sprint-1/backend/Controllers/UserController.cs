using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Models;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;
        
        public UserController()
        {
            userService = new UserService();
        }

        [HttpGet]
        public List<UserModel> Get()
        {
            var users = userService.GetUsers();
            return users;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email y password son requeridos.");

            // DEBUG seguro: email y longitud de password (no imprimir la contraseña)
            Console.WriteLine($"[DEBUG] Incoming login request. Email: '{request.Email}' PasswordLength: {request.Password?.Length ?? 0}");

            var user = userService.Authenticate(request.Email.Trim(), request.Password);
            if (user == null)
            {
                Console.WriteLine("[DEBUG] Authentication failed for email: " + request.Email);
                return Unauthorized("Credenciales inválidas.");
            }

            Console.WriteLine("[DEBUG] Authentication succeeded for user id: " + user.Id);
            return Ok(new { id = user.Id, email = user.Email });
        }
    }
}
