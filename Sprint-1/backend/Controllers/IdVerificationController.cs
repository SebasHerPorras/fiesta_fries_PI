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
    public class IdVerificationController: ControllerBase
    {
        public IdVerificationController() { }

        [HttpPost("idvalidate")]
        public ActionResult validate([FromBody] int id)
        {
            PersonRepository key = new PersonRepository();

            //ocupamos realizar el query
            PersonModel user = key.GetByIdentity(id);
            Console.WriteLine("LLega aquí\n");
            if (user == null)
            {
               return Ok(new {result = false });
            }
           
            return Ok(new {result = true});

        }
    }
}
