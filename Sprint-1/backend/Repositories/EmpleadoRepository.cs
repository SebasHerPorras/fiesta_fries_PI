using System;
using System.Collections.Generic;
using System.Linq;
using backend.Models;
using Dapper;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;

namespace backend.Repositories
{
    public class EmpleadoRepository
    {
        private readonly string _connectionString;

        public EmpleadoRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext") 
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");
        }

        public List<EmpleadoListDto> GetByEmpresa(long cedulaJuridica)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = @"
                SELECT 
                    p.id AS Cedula,
                    p.firstName + ' ' + p.secondName AS Nombre,
                    DATEDIFF(YEAR, p.birthdate, GETDATE()) AS Edad,
                    u.email AS Correo,
                    e.department AS Departamento,
                    e.employmentType AS TipoContrato
                FROM Empleado e
                INNER JOIN Persona p ON e.id = p.id
                INNER JOIN [User] u ON p.uniqueUser = u.PK_User
                WHERE e.idCompny = @CedulaJuridica
                ORDER BY e.department, p.firstName";

                return connection.Query<EmpleadoListDto>(query, new { CedulaJuridica = cedulaJuridica }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetByEmpresa: {ex.Message}");
                return new List<EmpleadoListDto>();
            }
        }
    }
}