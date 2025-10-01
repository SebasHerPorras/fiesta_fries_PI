using System;
using backend.Models;
using backend.Repositories;
using Dapper;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;

namespace backend.Services
{
    public class EmpleadoService
    {
        private readonly string _connectionString;
        private readonly PersonService _personService;

        public EmpleadoService()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext")
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");

            _personService = new PersonService();
        }

        // Reutiliza PersonService: si no existe la persona la crea (PersonService crea user si hace falta)
        public EmpleadoModel CreateEmpleadoWithPersonaAndUser(EmpleadoCreateRequest req)
        {
            try
            {
                // 1) comprobar si existe la persona por su id (identidad numérica)
                var existingPersona = _personService.GetByIdentity(req.personaId);

                PersonModel personaUsed;
                if (existingPersona != null)
                {
                    personaUsed = existingPersona;
                    Console.WriteLine($"Persona existente encontrada. Id persona: {personaUsed.id}, user: {personaUsed.uniqueUser}");
                }
                else
                {
                    // Construir PersonModel desde el request y delegar la creación a PersonService
                    var personToCreate = new PersonModel
                    {
                        id = req.personaId,
                        firstName = req.firstName,
                        secondName = req.secondName,
                        birthdate = req.birthdate,
                        direction = req.direction,
                        personalPhone = req.personalPhone,
                        homePhone = req.homePhone,
                        email = req.userEmail, // usar email para crear user dentro de PersonService
                        personType = req.personType ?? "Empleado",
                        uniqueUser = Guid.Empty
                    };

                    personaUsed = _personService.Insert(personToCreate);
                    Console.WriteLine($"Persona creada por PersonService: id={personaUsed.id}, uniqueUser={personaUsed.uniqueUser}");
                }

                // 2) insertar Empleado usando el id de la persona
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string insertEmpleado = @"INSERT INTO dbo.Empleado (id, position, employmentType) VALUES (@Id, @Position, @EmploymentType)";
                connection.Execute(insertEmpleado, new { Id = personaUsed.id, Position = req.position, EmploymentType = req.employmentType });
                Console.WriteLine($"Empleado creado para persona id={personaUsed.id}");

                return new EmpleadoModel
                {
                    id = personaUsed.id,
                    position = req.position,
                    employmentType = req.employmentType
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en CreateEmpleadoWithPersonaAndUser: " + ex);
                throw;
            }
        }

        public List<EmpleadoListDto> GetByEmpresa(long cedulaJuridica)
        {
            var empleadoRepository = new EmpleadoRepository();
            return empleadoRepository.GetByEmpresa(cedulaJuridica);
        }
    }
}