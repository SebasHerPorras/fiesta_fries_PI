using backend.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace backend.Repositories
{
    public class EmployeeDeductionsByPayrollRepository
    {
        private readonly string _connectionString;

        public EmployeeDeductionsByPayrollRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext") 
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");
        }

        public void SaveEmployeeDeductions(List<EmployeeDeductionsByPayrollDto> deductions)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                foreach (var deduction in deductions)
                {
                    const string query = @"
                        EXEC SP_InsertEmployeeDeductionsByPayroll 
                        @ReportId, @EmployeeId, @CedulaJuridicaEmpresa, 
                        @DeductionName, @DeductionAmount, @Percentage";

                    connection.Execute(query, new
                    {
                        ReportId = deduction.ReportId,
                        EmployeeId = deduction.EmployeeId,
                        CedulaJuridicaEmpresa = deduction.CedulaJuridicaEmpresa,
                        DeductionName = deduction.DeductionName,
                        DeductionAmount = deduction.DeductionAmount,
                        Percentage = deduction.Percentage
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving employee deductions: {ex.Message}", ex);
            }
        }
    }
}