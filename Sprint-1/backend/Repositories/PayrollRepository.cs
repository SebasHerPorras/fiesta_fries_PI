using Dapper;
using System.Data.SqlClient;
using backend.Models.Payroll;
using backend.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Repositories
{
    public class PayrollRepository : IPayrollRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<PayrollRepository> _logger;

        public PayrollRepository(
            IConfiguration configuration,
            ILogger<PayrollRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("UserContext")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string not found");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Payroll?> GetByPeriodAndCompanyAsync(DateTime periodDate, long companyId)
        {
            using var connection = CreateConnection();

            const string query = @"
                SELECT 
                    PayrollId, 
                    PeriodDate, 
                    CompanyId, 
                    IsCalculated, 
                    TotalAmount, 
                    ApprovedBy, 
                    LastModified
                FROM Payroll 
                WHERE PeriodDate = @PeriodDate AND CompanyId = @CompanyId";

            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<Payroll>(query, new
                {
                    PeriodDate = periodDate,
                    CompanyId = companyId
                });

                _logger.LogDebug("Consulta de planilla por período: {PeriodDate}, Compañía: {CompanyId} - {Result}",
                    periodDate.ToString("yyyy-MM"), companyId, result != null ? "Encontrada" : "No encontrada");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo planilla por período {PeriodDate} y compañía {CompanyId}",
                    periodDate.ToString("yyyy-MM"), companyId);
                throw;
            }
        }

        public async Task<int> CreatePayrollAsync(Payroll payroll)
        {
            using var connection = CreateConnection();

            const string query = @"
                INSERT INTO Payroll (PeriodDate, CompanyId, ApprovedBy, LastModified)
                OUTPUT INSERTED.PayrollId
                VALUES (@PeriodDate, @CompanyId, @ApprovedBy, @LastModified)";

            try
            {
                var payrollId = await connection.ExecuteScalarAsync<int>(query, new
                {
                    payroll.PeriodDate,
                    payroll.CompanyId,
                    payroll.ApprovedBy,
                    payroll.LastModified
                });

                _logger.LogInformation("Planilla creada exitosamente. ID: {PayrollId}, Período: {PeriodDate}",
                    payrollId, payroll.PeriodDate.ToString("yyyy-MM"));

                return payrollId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando planilla para período {PeriodDate} y compañía {CompanyId}",
                    payroll.PeriodDate.ToString("yyyy-MM"), payroll.CompanyId);
                throw;
            }
        }

        public async Task UpdatePayrollAsync(Payroll payroll)
        {
            using var connection = CreateConnection();

            const string query = @"
                UPDATE Payroll 
                SET IsCalculated = @IsCalculated,
                    TotalAmount = @TotalAmount,
                    LastModified = @LastModified
                WHERE PayrollId = @PayrollId";

            try
            {
                var affectedRows = await connection.ExecuteAsync(query, payroll);

                if (affectedRows == 0)
                {
                    _logger.LogWarning("No se encontró planilla con ID {PayrollId} para actualizar", payroll.PayrollId);
                    throw new InvalidOperationException($"Planilla con ID {payroll.PayrollId} no encontrada");
                }

                _logger.LogDebug("Planilla {PayrollId} actualizada exitosamente. Calculada: {IsCalculated}",
                    payroll.PayrollId, payroll.IsCalculated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando planilla {PayrollId}", payroll.PayrollId);
                throw;
            }
        }

        public async Task<List<PayrollPayment>> CreatePayrollPaymentsAsync(List<PayrollPayment> payments)
        {
            if (payments == null || !payments.Any())
            {
                _logger.LogWarning("Intento de crear pagos con lista vacía o nula");
                return new List<PayrollPayment>();
            }

            using var connection = CreateConnection();
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                const string procedureName = "sp_CreatePayrollPayment";
                var result = new List<PayrollPayment>();

                _logger.LogInformation("Iniciando creación de {PaymentCount} pagos mediante procedure {ProcedureName}",
                    payments.Count, procedureName);

                foreach (var payment in payments)
                {
                    var createdPayment = await ExecutePaymentProcedureAsync(
                        connection, transaction, procedureName, payment);

                    result.Add(createdPayment);
                }

                await transaction.CommitAsync();

                _logger.LogInformation("{PaymentCount} pagos creados exitosamente mediante procedure", payments.Count);
                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creando {PaymentCount} pagos. Transacción revertida.", payments.Count);
                throw;
            }
        }

        public async Task<List<PayrollPayment>> GetPayrollDetailsAsync(int payrollId)
        {
            using var connection = CreateConnection();

            const string query = @"
                SELECT 
                    PaymentId, 
                    PayrollId, 
                    EmployeeId, 
                    GrossSalary, 
                    DeductionsAmount, 
                    BenefitsAmount, 
                    NetSalary,
                    PaymentDate, 
                    Status
                FROM PayrollPayment 
                WHERE PayrollId = @PayrollId
                ORDER BY EmployeeId";

            try
            {
                var payments = await connection.QueryAsync<PayrollPayment>(query, new { PayrollId = payrollId });
                var paymentList = payments.AsList();

                _logger.LogDebug("Obtenidos {PaymentCount} detalles de pagos para planilla {PayrollId}",
                    paymentList.Count, payrollId);

                return paymentList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalles de pagos para planilla {PayrollId}", payrollId);
                throw;
            }
        }

        public async Task<bool> VerifyProcedureExistsAsync()
        {
            using var connection = CreateConnection();

            const string query = @"
                SELECT COUNT(*) 
                FROM sys.objects 
                WHERE object_id = OBJECT_ID('sp_CreatePayrollPayment') AND type = 'P'";

            try
            {
                var count = await connection.ExecuteScalarAsync<int>(query);
                var exists = count > 0;

                _logger.LogDebug("Verificación de procedure sp_CreatePayrollPayment: {Exists}",
                    exists ? "EXISTE" : "NO EXISTE");

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando existencia del procedure sp_CreatePayrollPayment");
                throw;
            }
        }

 
    }
}