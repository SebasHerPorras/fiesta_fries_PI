using backend.Models;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Data.Common;

namespace backend.Handlers.backend.Repositories
{
    public class BeneficioRepository
    {
        private readonly string _connectionString;

        public BeneficioRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext");
        }

        public string CreateBeneficio(BeneficioModel beneficio)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = @"INSERT INTO [dbo].[Beneficio] 
                              ([CedulaJuridica], [Nombre], [Tipo], [QuienAsume], 
                               [Valor], [Etiqueta])  
                              VALUES (@CedulaJuridica, @Nombre, @Tipo, @QuienAsume, 
                                      @Valor, @Etiqueta)";

                Console.WriteLine("-- Nuevo Beneficio: --");
                Console.WriteLine($"Cédula: {beneficio.CedulaJuridica}");
                Console.WriteLine($"Nombre: {beneficio.Nombre}");
                Console.WriteLine($"Tipo: {beneficio.Tipo}");
                Console.WriteLine($"QuienAsume: {beneficio.QuienAsume}");
                Console.WriteLine($"Valor: {beneficio.Valor}");
                Console.WriteLine($"Etiqueta: {beneficio.Etiqueta}");

                var affectedRows = connection.Execute(query, new
                {
                    CedulaJuridica = beneficio.CedulaJuridica,
                    Nombre = beneficio.Nombre,
                    Tipo = beneficio.Tipo,
                    QuienAsume = beneficio.QuienAsume,
                    Valor = beneficio.Valor,
                    Etiqueta = beneficio.Etiqueta
                });

                Console.WriteLine($"Filas afectadas: {affectedRows}");

                if (affectedRows >= 1)
                {
                    Console.WriteLine(" BENEFICIO INSERTADO (Rep)");
                    return string.Empty;
                }
                else
                {
                    return "Error al crear el beneficio";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN REPOSITORY BENEFICIO: {ex.Message}");
                return $"Error general: {ex.Message}";
            }
        }

        public List<BeneficioModel> GetAll()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                string query = "SELECT * FROM dbo.Beneficio";
                return connection.Query<BeneficioModel>(query).ToList(); ;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository GetBeneficios: {ex.Message}");
                throw;
            }
        }

        public List<BeneficioModel> GetByEmpresa(long cedulaJuridica)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                const string query = @"
                    SELECT * 
                    FROM Beneficio 
                    WHERE CedulaJuridica = @CedulaJuridica
                    ORDER BY Nombre";

                Console.WriteLine($"Buscando beneficios para empresa con cédula: {cedulaJuridica}");
                var beneficios = connection.Query<BeneficioModel>(query, new { CedulaJuridica = cedulaJuridica }).ToList();
                Console.WriteLine($"Se encontraron {beneficios.Count} beneficios para la empresa");

                return beneficios;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository GetByEmpresa: {ex.Message}");
                throw;
            }
        }

        public BeneficioModel GetById(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                const string query = @"
                    SELECT * 
                    FROM Beneficio 
                    WHERE IdBeneficio = @Id";

                Console.WriteLine($"Buscando beneficio con ID: {id}");
                var beneficio = connection.QueryFirstOrDefault<BeneficioModel>(query, new { Id = id });

                if (beneficio != null)
                    Console.WriteLine($"Beneficio encontrado: {beneficio.Nombre}");
                else
                    Console.WriteLine($"Beneficio con ID {id} no encontrado");

                return beneficio;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository GetById: {ex.Message}");
                throw;
            }
        }

        public bool Update(BeneficioModel beneficio)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);

                const string query = @"
                    UPDATE Beneficio
                    SET 
                        Nombre = @Nombre,
                        Tipo = @Tipo,
                        QuienAsume = @QuienAsume,
                        Valor = @Valor,
                        Etiqueta = @Etiqueta
                    WHERE IdBeneficio = @IdBeneficio";

                Console.WriteLine($"Actualizando beneficio ID: {beneficio.IdBeneficio}");

                var filasAfectadas = connection.Execute(query, new
                {
                    beneficio.Nombre,
                    beneficio.Tipo,
                    beneficio.QuienAsume,
                    beneficio.Valor,
                    beneficio.Etiqueta,
                    beneficio.IdBeneficio
                });

                Console.WriteLine($"Filas afectadas: {filasAfectadas}");
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Repository Update: {ex.Message}");
                return false;
            }
        }

    }
}