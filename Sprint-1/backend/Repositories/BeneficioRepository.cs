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
        private readonly ILogger<BeneficioRepository> _logger;

        public BeneficioRepository(ILogger<BeneficioRepository> logger)
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext");
            _logger = logger;
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

                _logger.LogInformation("Insertando nuevo beneficio: {Nombre}", beneficio.Nombre);

                var affectedRows = connection.Execute(query, new
                {
                    CedulaJuridica = beneficio.CedulaJuridica,
                    Nombre = beneficio.Nombre,
                    Tipo = beneficio.Tipo,
                    QuienAsume = beneficio.QuienAsume,
                    Valor = beneficio.Valor,
                    Etiqueta = beneficio.Etiqueta
                });

                _logger.LogInformation("Filas afectadas: {Rows}", affectedRows);

                if (affectedRows >= 1)
                {
                    return string.Empty;
                }
                else
                {
                    return "Error al crear el beneficio";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CreateBeneficio");
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
                _logger.LogError(ex, "Error en GetAll");
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

                _logger.LogInformation("Buscando beneficios para empresa {Cedula}", cedulaJuridica);
                var beneficios = connection.Query<BeneficioModel>(query, new { CedulaJuridica = cedulaJuridica }).ToList();
                _logger.LogInformation("Se encontraron {Count} beneficios", beneficios.Count);

                return beneficios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetByEmpresa");
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

                _logger.LogInformation("Buscando beneficio con ID {Id}", id);
                var beneficio = connection.QueryFirstOrDefault<BeneficioModel>(query, new { Id = id });

                if (beneficio != null)
                    _logger.LogInformation("Beneficio encontrado: {Nombre}", beneficio.Nombre);
                else
                    _logger.LogWarning("Beneficio con ID {Id} no encontrado", id);

                return beneficio;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GetById");
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

                _logger.LogInformation("Actualizando beneficio ID {Id}", beneficio.IdBeneficio);

                var filasAfectadas = connection.Execute(query, new
                {
                    beneficio.Nombre,
                    beneficio.Tipo,
                    beneficio.QuienAsume,
                    beneficio.Valor,
                    beneficio.Etiqueta,
                    beneficio.IdBeneficio
                });

                _logger.LogInformation("Filas afectadas: {Rows}", filasAfectadas);
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Update");
                return false;
            }
        }

        public bool ExistsInEmployerBenefitDeductions(int benefitId)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = @"SELECT COUNT(1)
                                   FROM EmployerBenefitDeductions 
                                   WHERE BenefitId = @BenefitId";

            var count = connection.ExecuteScalar<int>(query, new { BenefitId = benefitId });
            _logger.LogInformation("Checking EmployerBenefitDeductions for benefit {BenefitId}: {Count}", benefitId, count);
            return count > 0;
        }

        public void PhysicalDeletion(int benefitId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("SP_Benefit_PhysicalDeletion",
                new { IdBeneficio = benefitId },
                commandType: CommandType.StoredProcedure);
            _logger.LogInformation("Physical deletion executed for benefit {BenefitId}", benefitId);
        }

        public void LogicalDeletion(int benefitId, DateTime? lastPeriod)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Execute("SP_Benefit_LogicalDeletion",
                new { IdBeneficio = benefitId, LastPeriod = lastPeriod },
                commandType: CommandType.StoredProcedure);
            _logger.LogInformation("Logical deletion executed for benefit {BenefitId}, last period {LastPeriod}", benefitId, lastPeriod);
        }

    }
}