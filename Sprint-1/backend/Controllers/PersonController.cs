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

            service.Insert(person);
            return Ok(new { message = "Persona creada exitosamente" });
        }
    }

}
