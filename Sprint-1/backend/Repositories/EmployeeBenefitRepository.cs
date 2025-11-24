using backend.Models;
using backend.Interfaces;
using System.Threading.Tasks;
using System;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace backend.Repositories
{
    public class EmployeeBenefitRepository : IEmployeeBenefitRepository
    {
        private readonly IDbConnection _db;

        public EmployeeBenefitRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("UserContext"));
        }

        public async Task<List<int>> GetSelectedBenefitIdsAsync(int employeeId)
        {
            var sql = @"SELECT benefitId
                       FROM EmployeeBenefit
                       WHERE employeeId = @employeeId
                        AND IsDeleted = 0";
            var result = await _db.QueryAsync<int>(sql, new { employeeId });
            return result.AsList();
        }

        public async Task<bool> SaveSelectionAsync(EmployeeBenefit entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            if (entity.EmployeeId <= 0 || entity.BenefitId <= 0) throw new ArgumentException("EmployeeId y BenefitId requeridos");

            const string sql = @"
                INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType)
                VALUES (@EmployeeId, @BenefitId, @PensionType, @DependentsCount, @ApiName, @BenefitValue, @BenefitType);";

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
                    DependentsCount = entity.DependentsCount,
                    ApiName = entity.ApiName,
                    BenefitValue = entity.BenefitValue,
                    BenefitType = entity.BenefitType
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

        public async Task<BeneficioModel?> GetBeneficioByIdAsync(int beneficioId)
        {
            if (beneficioId <= 0) return null;

            const string sql = @"
                SELECT TOP 1
                    idBeneficio       AS IdBeneficio,
                    cedulaJuridica    AS CedulaJuridica,
                    nombre            AS Nombre,
                    tipo              AS Tipo,
                    quienAsume        AS QuienAsume,
                    valor             AS Valor,
                    etiqueta          AS Etiqueta
                FROM Beneficio
                WHERE idBeneficio = @BeneficioId;";

            // Abrir la conexión si está cerrada
            if (_db is System.Data.SqlClient.SqlConnection sqlConn && sqlConn.State == System.Data.ConnectionState.Closed)
            {
                await sqlConn.OpenAsync();
            }

            var result = await _db.QueryFirstOrDefaultAsync<BeneficioModel>(sql, new { BeneficioId = beneficioId });
            return result;
        }

        public async Task<List<EmployeeBenefit>> GetSelectedByEmployeeIdAsync(int employeeId)
        {
            if (employeeId < 0) return new List<EmployeeBenefit>();

            const string sql = @"
                SELECT
                    employeeId        AS EmployeeId,
                    benefitId         AS BenefitId,
                    pensionType       AS PensionType,
                    dependentsCount   AS DependentsCount,
                    apiName           AS ApiName,
                    benefitValue      AS BenefitValue,
                    benefitType       AS BenefitType
                FROM EmployeeBenefit
                WHERE employeeId = @EmployeeId
                    AND IsDeleted = 0;";

            if (_db is System.Data.SqlClient.SqlConnection sqlConn && sqlConn.State == System.Data.ConnectionState.Closed)
            {
                await sqlConn.OpenAsync();
            }

            var result = await _db.QueryAsync<EmployeeBenefit>(sql, new { EmployeeId = employeeId });
            return result.AsList();
        }

        public int CountBeneficiosPorEmpleado(int employeeId)
        {
            const string query = @"
            SELECT COUNT(*) 
            FROM EmployeeBenefit 
            WHERE EmployeeId = @EmployeeId
                AND IsDeleted = 0; ";

            return _db.ExecuteScalar<int>(query, new { EmployeeId = employeeId });
        }

    }

}
