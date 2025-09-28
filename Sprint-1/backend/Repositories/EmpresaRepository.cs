using backend.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Data.Common;

namespace backend.Handlers.backend.Repositories
{
    public class EmpresaRepository
    {
        private readonly string _connectionString;

        public EmpresaRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("CountryContext");
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
                    return "EMPRESA_CREADA_EXITOSAMENTE";
                }
                else
                {
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
    }
}