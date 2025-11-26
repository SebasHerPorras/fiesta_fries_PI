using backend.Models;
using backend.Interfaces;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Data.Common;

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
                    FROM Empresa e
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

                var query = @"INSERT INTO [dbo].[Empresa] 
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
            string query = "SELECT * FROM dbo.Empresa";
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
            FROM Empresa e
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
                  FROM Empresa e
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
                  FROM Empresa e
                  INNER JOIN Empleado emp ON e.CedulaJuridica = emp.idCompny
                  INNER JOIN Persona p ON emp.id = p.id
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
                    FROM Empresa 
                    WHERE CedulaJuridica = @CedulaJuridica";

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
                UPDATE Empresa
                SET Nombre = @Nombre,
                    DireccionEspecifica = @DireccionEspecifica,
                    Telefono = @Telefono,
                    NoMaxBeneficios = @NoMaxBeneficios,
                    FrecuenciaPago = @FrecuenciaPago,
                    DiaPago = @DiaPago
                WHERE CedulaJuridica = @CedulaJuridica";

            connection.Execute(query, empresa);
        }
        public int CheckCompanyPayroll(EmpresaModel empresa)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
                SELECT CASE 
                    WHEN EXISTS (
                        SELECT 1 
                        FROM payroll 
                        WHERE companyid = @CedulaJuridica
                    ) THEN 1 
                    ELSE 0 
                END";

            return connection.ExecuteScalar<int>(query, new { empresa.CedulaJuridica });
        }
        public int DeleteEmpresa(long cedulaJuridica, bool physicalDelete)
        {
            using var connection = new SqlConnection(_connectionString);

            string query = physicalDelete
                ? @"UPDATE Empresa SET isDeleted = 1 WHERE CedulaJuridica = @CedulaJuridica"
                : @"DELETE FROM Empresa WHERE CedulaJuridica = @CedulaJuridica";


            return connection.Execute(query, new { CedulaJuridica = cedulaJuridica });
        }

    }
}
