using backend.Models;
using backend.Services;
using backend.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Dapper; 

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TEST_SELENIUM : ControllerBase
    {
        private readonly string _connectionString;

        public TEST_SELENIUM(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("UserContext");
        }

        [HttpGet("empresa/{empresaId}/ultimo-cedula")]
        public ActionResult<string> GetUltimaCedulaEmpleado(long empresaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT TOP 1 id FROM [Fiesta_Fries_DB].[Empleado] WHERE idCompny = @empresaId ORDER BY id DESC";
                var cedula = connection.QueryFirstOrDefault<string>(query, new { empresaId });
                return Ok(cedula ?? "0");
            }
        }
    }
}