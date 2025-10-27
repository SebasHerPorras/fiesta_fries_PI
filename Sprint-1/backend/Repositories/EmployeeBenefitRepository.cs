using backend.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class EmployeeBenefitRepository
    {
        private readonly IDbConnection _db;

        public EmployeeBenefitRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("UserContext"));
        }

        public async Task<List<int>> GetSelectedBenefitIdsAsync(int employeeId)
        {
            var sql = "SELECT benefitId FROM EmployeeBenefit WHERE employeeId = @employeeId";
            var result = await _db.QueryAsync<int>(sql, new { employeeId });
            return result.AsList();
        }

        public async Task<bool> SaveSelectionAsync(EmployeeBenefit entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.EmployeeId <= 0 || entity.BenefitId <= 0) throw new ArgumentException("EmployeeId y BenefitId requeridos");

            const string sql = @"
                INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount)
                VALUES (@EmployeeId, @BenefitId, @PensionType, @DependentsCount);";

            // Abrir la conexión si está cerrada
            if (_db is System.Data.SqlClient.SqlConnection sqlConn && sqlConn.State == System.Data.ConnectionState.Closed)
            {
                await sqlConn.OpenAsync();
            }

            using var tx = _db.BeginTransaction();
            try
            {
                var rows = await _db.ExecuteAsync(sql, new
                {
                    EmployeeId = entity.EmployeeId,
                    BenefitId = entity.BenefitId,
                    PensionType = entity.PensionType,
                    DependentsCount = entity.DependentsCount
                }, tx);

                tx.Commit();
                return rows > 0;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<bool> CanEmployeeSelectBenefitAsync(int employeeId, int benefitId)
        {
            const string sql = "SELECT CAST(dbo.CanEmployeeSelectBenefit(@EmployeeId," +
                "                   @BenefitId) AS INT) AS CanSelect";
            var result = await _db.QueryFirstOrDefaultAsync<int?>(sql, new { EmployeeId = employeeId, BenefitId = benefitId });
            return result.HasValue && result.Value == 1;
        }

    }

}
