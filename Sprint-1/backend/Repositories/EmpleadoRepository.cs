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
                return new List<EmpleadoListDto>();
            }
        }

        public DateTime GetHireDate(int id_)
        {
            using var connection = new SqlConnection(this._connectionString);

            const string query = "SELECT hiredate from Empleado where id = @id";

            return connection.QuerySingleOrDefault<DateTime>(query, new {id = id_});
        }

        public WeekEmployeeModel? GetWorkWeek(DateTime date_, int idEmployee)
        {
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"EXEC sp_GetOrCreateWeek @start_date = @start_date, @id_employee = @id_employee";

            return connection.QuerySingleOrDefault<WeekEmployeeModel>(query, new {start_date = date_, id_employee = idEmployee});
        }
        
        public EmployeeWorkDayModel? GetWorkDay(DateTime weekDate_,DateTime dayDate_, int idEmployee)
        {
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"EXEC sp_GetOrCreateDay @date = @date, @week_start_date = @week_start_date, @id_employee = @id_employee";

            return connection.QuerySingleOrDefault<EmployeeWorkDayModel>(query, new {date = dayDate_, week_start_date=weekDate_,id_employee = idEmployee});
        }

        public EmployeeWorkDayModel? AddHours(DateTime dateW,DateTime dateD,int hours_, int idEmployee)
        {
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"EXEC sp_AddHoursToDayAndGetTable @hours_count = @hours_count, @date = @date,@week_start_date = @week_start_date ,@id_employee = @id_employee";

            return connection.QuerySingleOrDefault<EmployeeWorkDayModel>(query, new { date = dateD, hours_count = hours_ ,id_employee=idEmployee,week_start_date = dateW});
        }

        public List<EmployeeCalculationDto> GetEmployeesForPayroll(long cedulaJuridica, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    EXEC SP_GetEmployeesForPayroll 
                    @CedulaJuridica, @FechaInicio, @FechaFin";

                return connection.Query<EmployeeCalculationDto>(query, new 
                { 
                    CedulaJuridica = cedulaJuridica,
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo empleados para planilla: {ex.Message}");
                return new List<EmployeeCalculationDto>();
            }
        }

        public List<EmpleadoModel> GetEmpleadosPorEmpresa(long cedulaEmpresa)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var query = @"
            SELECT 
                id, position, employmentType, salary, hireDate, department, idCompny,
                '' AS email, '' AS name
            FROM Empleado
            WHERE idCompny = @CedulaEmpresa";

            return connection.Query<EmpleadoModel>(query, new { CedulaEmpresa = cedulaEmpresa }).ToList();
        }

        public EmpleadoModel? GetById(int id_)
        {
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"SELECT* FROM Empleado WHERE id = @id";
            Console.WriteLine("Querry realizado con éxito\n");

            return connection.QuerySingleOrDefault<EmpleadoModel>(query, new { id = id_ });
        }

        public async Task UpdateAsync(EmpleadoModel empleado)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = @"
                UPDATE Empleado SET
                    position = @Position,
                    department = @Department,
                    salary = @Salary
                WHERE id = @Id";

            await connection.ExecuteAsync(query, new
            {
                empleado.position,
                empleado.department,
                empleado.salary,
                empleado.id
            });
        }

        public async Task<EmpleadoUpdateDto?> GetEmpleadoPersonaByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = @"
                SELECT 
                    e.id,
                    p.firstName,
                    p.secondName,
                    p.direction,
                    p.personalPhone,
                    p.homePhone,
                    e.position,
                    e.department,
                    e.salary
                FROM Empleado e
                INNER JOIN Persona p ON e.id = p.id
                WHERE e.id = @id";

            return await connection.QuerySingleOrDefaultAsync<EmpleadoUpdateDto>(query, new { id });
        }

    }
}