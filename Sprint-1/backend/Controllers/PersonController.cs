using backend.Models;
using backend.Repositories;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonController : ControllerBase
    {
        private PersonService service;
        public PersonController() {
            this.service = new PersonService();
        }

        [HttpPost("create")]
        public ActionResult createPerson([FromBody] PersonModel person)
        {
            if (person == null)
            {
                return BadRequest("Datos inválidos");
            }

            var created = service.Insert(person);
            return Ok(created); // ahora retorna la persona creada (incluye uniqueUser y id)
        }

        [HttpGet("profile/{userId:guid}")]
        public ActionResult getPersonalProfile(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("ID de usuario inválido");
            }

            var profile = service.GetPersonalProfile(userId);
            if (profile == null)
            {
                return NotFound("Perfil no encontrado");
            }

            return Ok(profile);
        }

    }

}
