using backend.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper;

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

        public bool CreateEmpresa(EmpresaModel empresa)
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
                Console.WriteLine($"DueñoEmpresa: {empresa.DueñoEmpresa}");
                Console.WriteLine($"Query: {query}");

                var affectedRows = connection.Execute(query, new
                {
                    CedulaJuridica = empresa.CedulaJuridica,
                    Nombre = empresa.Nombre,
                    DueñoEmpresa = empresa.DueñoEmpresa, 
                    DireccionEspecifica = empresa.DireccionEspecifica,
                    Telefono = empresa.Telefono,
                    NoMaxBeneficios = empresa.NoMaxBeneficios,
                    FrecuenciaPago = empresa.FrecuenciaPago,
                    DiaPago = empresa.DiaPago
                });

                Console.WriteLine($"Filas afectadas: {affectedRows}");

                return affectedRows >= 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN REPOSITORY: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw; 
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