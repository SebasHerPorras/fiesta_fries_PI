using System;
using backend.Models;
using backend.Repositories;
using backend.Interfaces.Services;
using Dapper;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.Services
{
    public class EmpleadoService : IEmployeeService
    {
        private readonly string _connectionString;
        private readonly PersonService _personService;
        private readonly PersonRepository _personRepository;
        private readonly EmpleadoRepository _empleadoService;

        public EmpleadoService()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext")
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");

            _personService = new PersonService();
            _personRepository = new PersonRepository();
            _empleadoService = new EmpleadoRepository(); 
        }

        // Reutiliza PersonService: si no existe la persona la crea (PersonService crea user si hace falta)
        public EmpleadoModel CreateEmpleadoWithPersonaAndUser(EmpleadoCreateRequest req)
        {
            try
            {
                // 1) comprobar si existe la persona por su id (identidad num�rica)
                var existingPersona = _personService.GetByIdentity(req.personaId);

                PersonModel personaUsed;
                if (existingPersona != null)
                {
                    personaUsed = existingPersona;
                    Console.WriteLine($"Persona existente encontrada. Id persona: {personaUsed.id}, user: {personaUsed.uniqueUser}");
                }
                else
                {
                    // Construir PersonModel desde el request y delegar la creaci�n a PersonService
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
                var empleadoD = new EmpleadoModel
                {
                    id = personaUsed.id,
                    position = req.position,
                    employmentType = req.employmentType,
                    salary = req.salary,
                    hireDate = req.hireDate,
                    department = req.departament,
                    idCompny = req.idCompny
                };

                const string insertEmpleado = @"INSERT INTO dbo.Empleado (id, position, employmentType,salary,hireDate,department,idCompny) VALUES (@id, @position, @employmentType, @salary,@hireDate,@department,@idCompny)";
                connection.Execute(insertEmpleado, empleadoD);
                Console.WriteLine($"Empleado creado para persona id={personaUsed.id}");

                return empleadoD;
            
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

        public async Task<decimal> GetSalarioBrutoAsync(int cedulaEmpleado)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = "SELECT salary FROM Empleado WHERE id = @Cedula";
            var salario = await connection.ExecuteScalarAsync<int?>(query, new { Cedula = cedulaEmpleado });
            return salario ?? 0;
        }

        public List<EmployeeCalculationDto> GetEmployeeCalculationDtos(long cedulaJuridica, DateTime fechaInicio, DateTime fechaFin)
        {
            Console.WriteLine("=== DEBUG GetEmployeeCalculationDtos ===");
            Console.WriteLine("Parametros recibidos:");
            Console.WriteLine("  - Cedula Juridica: " + cedulaJuridica);
            Console.WriteLine("  - Fecha Inicio: " + fechaInicio.ToString("yyyy-MM-dd"));
            Console.WriteLine("  - Fecha Fin: " + fechaFin.ToString("yyyy-MM-dd"));

            try
            {
                Console.WriteLine("Llamando a _empleadoService.GetEmployeesForPayroll()...");
                var resultado = _empleadoService.GetEmployeesForPayroll(cedulaJuridica, fechaInicio, fechaFin);

                Console.WriteLine("Metodo completado exitosamente");
                Console.WriteLine("Registros retornados: " + (resultado?.Count ?? 0));

                if (resultado == null || resultado.Count == 0)
                {
                    Console.WriteLine("La lista retornada esta vacia o es nula");
                }
                else
                {
                    foreach (var emp in resultado)
                    {
                        Console.WriteLine("EMPLEADO: " + emp.NombreEmpleado);
                        Console.WriteLine("  horas: " + emp.horas); 
                        Console.WriteLine("  ---");
                    }

                    var conHoras = resultado.Count(e => e.horas > 0);
                    var sinHoras = resultado.Count(e => e.horas == 0);

                    Console.WriteLine("ESTADISTICAS:");
                    Console.WriteLine("  - Empleados con horas > 0: " + conHoras);
                    Console.WriteLine("  - Empleados sin horas: " + sinHoras);

                }

                Console.WriteLine("=== FIN DEBUG ===");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                return new List<EmployeeCalculationDto>();
            }
        }

        public DateTime GetHireDate(int id)
        {
            DateTime hireDate = this._empleadoService.GetHireDate(id);

            return hireDate;
        }

        public async Task<bool> UpdateEmpleadoAsync(int id, EmpleadoUpdateDto dto)
        {
            var empleado = _empleadoService.GetById(id);
            if (empleado == null)
                return false;

            var persona = _personRepository.GetByIdentity(id);
            if (persona == null)
                return false;

            // Datos de Persona
            persona.firstName = dto.FirstName;
            persona.secondName = dto.SecondName;
            persona.direction = dto.Direction;
            persona.personalPhone = dto.PersonalPhone;
            persona.homePhone = dto.HomePhone;

            // Datos de Empleado
            empleado.position = dto.Position;
            empleado.department = dto.Department;
            empleado.salary = dto.Salary;

            _personRepository.Update(persona);
            await _empleadoService.UpdateAsync(empleado);

            return true;
        }
        public async Task<EmpleadoUpdateDto?> GetEmpleadoPersonaByIdAsync(int id)
        {
            return await _empleadoService.GetEmpleadoPersonaByIdAsync(id);
        }


    }
}