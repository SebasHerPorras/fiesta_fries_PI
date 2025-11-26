using backend.Interfaces;
using backend.Models;
using Dapper;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Drawing;

namespace backend.Handlers.backend.Repositories
{
    public class EmpresaRepository : IEmpresaRepository 
    {
        private readonly string _connectionString;

        public EmpresaRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext");
        }

        public EmpresaModel GetByCedulaJuridica(long cedulaJuridica)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                const string query = @"
                    SELECT e.*, 
                           0 as CantidadEmpleados
                    FROM [Fiesta_Fries_DB].[Empresa] e
                    WHERE e.CedulaJuridica = @CedulaJuridica
                    AND e.isDeleted = 0";


                Console.WriteLine($"Buscando empresa con cédula: {cedulaJuridica}");
                var empresa = connection.QueryFirstOrDefault<EmpresaModel>(query, new { CedulaJuridica = cedulaJuridica });

                if (empresa != null)
                    Console.WriteLine($"Empresa encontrada: {empresa.Nombre}");
                else
                    Console.WriteLine($"Empresa con cédula {cedulaJuridica} no encontrada");

                return empresa;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository GetByCedulaJuridica: {ex.Message}");
                throw;
            }
        }

        public string CreateEmpresa(EmpresaModel empresa)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = @"INSERT INTO [Fiesta_Fries_DB].[Empresa] 
                              ([CedulaJuridica], [Nombre], [DueñoEmpresa], [DireccionEspecifica], 
                               [Telefono], [NoMaxBeneficios], [FrecuenciaPago], [DiaPago]) 
                              VALUES (@CedulaJuridica, @Nombre, @DueñoEmpresa, @DireccionEspecifica, 
                                      @Telefono, @NoMaxBeneficios, @FrecuenciaPago, @DiaPago)";

                Console.WriteLine("=== EJECUTANDO INSERT ===");
                Console.WriteLine($"Cédula: {empresa.CedulaJuridica}");
                Console.WriteLine($"DueñoEmpresa: {empresa.DueñoEmpresa}");

                var affectedRows = connection.Execute(query, new
                {
                    CedulaJuridica = empresa.CedulaJuridica,
                    Nombre = empresa.Nombre,
                    DueñoEmpresa = empresa.DueñoEmpresa,
                    DireccionEspecifica = (object)empresa.DireccionEspecifica ?? DBNull.Value,
                    Telefono = empresa.Telefono.HasValue ? (object)empresa.Telefono.Value : DBNull.Value,
                    NoMaxBeneficios = empresa.NoMaxBeneficios,
                    FrecuenciaPago = empresa.FrecuenciaPago,
                    DiaPago = empresa.DiaPago
                });

                Console.WriteLine($"Filas afectadas: {affectedRows}");

                if (affectedRows >= 1)
                {
                    Console.WriteLine("INSERT EXITOSO EN REPOSITORY");
                    return string.Empty; 
                }
                else
                {
                    Console.WriteLine("No se insertaron filas");
                    return "No se pudo crear la empresa";
                }
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                Console.WriteLine($"ERROR: Cédula jurídica duplicada - {empresa.CedulaJuridica}");
                return "Ya existe una empresa con esa cédula jurídica.";
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"ERROR SQL: {ex.Message} (Número: {ex.Number})");
                return $"Error de base de datos: {ex.Message}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN REPOSITORY: {ex.Message}");
                return $"Error general: {ex.Message}";
            }
        }

        public List<EmpresaModel> GetEmpresas()
        {
            using var connection = new SqlConnection(_connectionString);
            string query = "SELECT * FROM [Fiesta_Fries_DB].[Empresa]";
            return connection.Query<EmpresaModel>(query).ToList();
        }

        public List<EmpresaModel> GetByOwner(int ownerId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                const string query = @"
            SELECT e.*, 
                   0 as CantidadEmpleados  -- Por ahora 0, luego puedes contar empleados reales
            FROM [Fiesta_Fries_DB].[Empresa] e
            WHERE e.DueñoEmpresa = @OwnerId
            ORDER BY e.Nombre";

                Console.WriteLine($"Buscando empresas para DueñoEmpresa: {ownerId}");
                var empresas = connection.Query<EmpresaModel>(query, new { OwnerId = ownerId }).ToList();
                Console.WriteLine($"Se encontraron {empresas.Count} empresas");

                return empresas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository GetByOwner: {ex.Message}");
                throw;
            }
        }

        public EmpresaModel GetById(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                const string query = @"
                  SELECT e.*, 
                        0 as CantidadEmpleados
                  FROM [Fiesta_Fries_DB].[Empresa] e
                  WHERE e.id = @Id";

                Console.WriteLine($"Buscando empresa con ID: {id}");
                var empresa = connection.QueryFirstOrDefault<EmpresaModel>(query, new { Id = id });

                if (empresa != null)
                    Console.WriteLine($"Empresa encontrada: {empresa.Nombre}");
                else
                    Console.WriteLine($"Empresa con ID {id} no encontrada");

                return empresa;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository GetById: {ex.Message}");
                throw;
            }
        }
        public EmpresaModel GetByEmployeeUserId(string userId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                const string query = @"
                  SELECT e.*
                  FROM [Fiesta_Fries_DB].[Empresa] e
                  INNER JOIN [Fiesta_Fries_DB].[Empleado] emp ON e.CedulaJuridica = emp.idCompny
                  INNER JOIN [Fiesta_Fries_DB].[Persona] p ON emp.id = p.id
                  WHERE p.uniqueUser = @UserId";

                Console.WriteLine($"Buscando empresa para empleado con UserId (uniqueUser): {userId}");
                var empresa = connection.QueryFirstOrDefault<EmpresaModel>(query, new { UserId = Guid.Parse(userId) });

                if (empresa != null)
                    Console.WriteLine($"Empresa encontrada: {empresa.Nombre}");
                else
                    Console.WriteLine($"No se encontró empresa para el empleado con UserId: {userId}");

                return empresa;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository GetByEmployeeUserId: {ex.Message}");
                throw;
            }


        }

        public EmpresaModel GetByCedula(long cedulaJuridica)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                const string query = @"
                    SELECT * 
                    FROM [Fiesta_Fries_DB].[Empresa] WHERE CedulaJuridica = @CedulaJuridica";

                Console.WriteLine($"Buscando empresa por cédula jurídica: {cedulaJuridica}");

                var empresa = connection.QueryFirstOrDefault<EmpresaModel>(query, new { CedulaJuridica = cedulaJuridica });

                if (empresa != null)
                    Console.WriteLine($"Empresa encontrada: {empresa.Nombre}");
                else
                    Console.WriteLine("No se encontró empresa con esa cédula jurídica");

                return empresa;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository GetByCedula: {ex.Message}");
                throw;
            }
        }

        public void Update(EmpresaModel empresa)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
                UPDATE [Fiesta_Fries_DB].[Empresa] SET Nombre = @Nombre,
                    DireccionEspecifica = @DireccionEspecifica,
                    Telefono = @Telefono,
                    NoMaxBeneficios = @NoMaxBeneficios,
                    FrecuenciaPago = @FrecuenciaPago,
                    DiaPago = @DiaPago
                WHERE CedulaJuridica = @CedulaJuridica";

            connection.Execute(query, empresa);
        }

        public List<EmploymentTypeCountModel>? GetEmpleadosCountByRoll(long id_)
        {
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"SELECT e.employmentType, COUNT(e.employmentType) AS cantidadEmpleados
                                 FROM [Fiesta_Fries_DB].[Empleado] e inner JOIN [Fiesta_Fries_DB].[Empresa] em on e.idCompny = em.CedulaJuridica where em.CedulaJuridica = @id
                                  AND e.isDeleted = 0 group by e.employmentType";

            var result = connection.Query<EmploymentTypeCountModel>(query, new { id = id_ })
                         ?.ToList();
            if (result == null)
            {
                return null;
            }

            return result;
        }

        public List<DateTime> GetUltimasFechasPago(long cedulaJuridica, DateTime fechaLimite)
        {
            using var connection = new SqlConnection(_connectionString);

            Console.WriteLine("Cedula" + cedulaJuridica);
            Console.WriteLine("Fecha" + fechaLimite);

            const string query = @"
             SELECT DISTINCT TOP 3 
             CONVERT(date, CreatedDate) as FechaPago
             FROM [Fiesta_Fries_DB].[EmployerSocialSecurityByPayroll] WHERE CedulaJuridicaEmpresa = @cedulaJuridica 
             AND CONVERT(date, CreatedDate) <= @fechaLimite
             ORDER BY CONVERT(date, CreatedDate) DESC";

            List<DateTime> result = connection.Query<DateTime>(query, new
             {
                cedulaJuridica = cedulaJuridica,
                fechaLimite = fechaLimite.Date
             })?.ToList();

            Console.WriteLine("Datos: " + result.Count);
            return result;
        }

        public decimal getEmployerDeductions(long id, DateTime fecha)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
          SELECT 
          ISNULL(SUM(es.Amount), 0) AS CostoTotal
          FROM [Fiesta_Fries_DB].[EmployerSocialSecurityByPayroll] es
          INNER JOIN [Fiesta_Fries_DB].[Empleado] e ON es.EmployeeId = e.id 
          WHERE es.CedulaJuridicaEmpresa = @Cedula
          AND e.IsDeleted = 0
          AND CONVERT(date, es.CreatedDate) = @Fecha;";

            return connection.QuerySingleOrDefault<decimal>(query, new { Cedula = id, Fecha = fecha });

        }

        public decimal GetBeneficiosEmpresa(long cedula)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
            SELECT 
            ISNULL(SUM(b.Valor), 0) AS MontoTotal
            FROM [Fiesta_Fries_DB].[Beneficio] b
            INNER JOIN [Fiesta_Fries_DB].[Empresa] e ON b.CedulaJuridica = e.CedulaJuridica
            WHERE e.CedulaJuridica = @Cedula
            AND b.QuienAsume = 'Empresa'
            AND b.IsDeleted = 0;";

            return connection.ExecuteScalar<decimal>(query, new { Cedula = cedula });
        }

        public decimal GetTotalSalarios(long cedula)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
              SELECT 
              ISNULL(SUM(e.salary), 0) AS SalarioTotal
             FROM [Fiesta_Fries_DB].[Empleado] e
             WHERE e.idCompny = @Cedula
             AND e.isDeleted = 0;";

            return connection.ExecuteScalar<decimal>(query, new { Cedula = cedula });
        }




        public int CheckCompanyPayroll(EmpresaModel empresa)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
                SELECT CASE 
                    WHEN EXISTS (
                        SELECT 1 
                        FROM [Fiesta_Fries_DB].[payroll] WHERE companyid = @CedulaJuridica
                    ) THEN 1 
                    ELSE 0 
                END";

            return connection.ExecuteScalar<int>(query, new { empresa.CedulaJuridica });
        }
        public int DeleteEmpresa(long cedulaJuridica, bool physicalDelete)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = physicalDelete
                ? @"UPDATE [Fiesta_Fries_DB].[Empresa] SET isDeleted = 1 WHERE CedulaJuridica = @CedulaJuridica"
                : @"DELETE FROM [Fiesta_Fries_DB].[Empresa] WHERE CedulaJuridica = @CedulaJuridica";


            return connection.Execute(query, new { CedulaJuridica = cedulaJuridica });
        }

    }
}
