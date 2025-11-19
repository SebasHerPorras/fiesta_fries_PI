using System.Data.SqlClient;
using backend.Models;
using Dapper;

namespace backend.Repositories
{
    public class EmployerBenefitDeductionRepository
    {
        private readonly string _connectionString;

        public EmployerBenefitDeductionRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext") 
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");
        }

        public void SaveEmployerBenefitDeductions(List<EmployerBenefitDeductionDto> deductions)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                foreach (var deduction in deductions)
                {
                    const string query = @"
                        EXEC SP_InsertEmployerBenefitDeduction 
                        @ReportId, @EmployeeId, @CedulaJuridicaEmpresa,
                        @BenefitName, @BenefitId, @DeductionAmount, @BenefitType, @Percentage";

                    connection.Execute(query, new
                    {
                        ReportId = deduction.ReportId,
                        EmployeeId = deduction.EmployeeId,
                        CedulaJuridicaEmpresa = deduction.CedulaJuridicaEmpresa,
                        BenefitName = deduction.BenefitName,
                        BenefitId = deduction.BenefitId,
                        DeductionAmount = deduction.DeductionAmount,
                        BenefitType = deduction.BenefitType,
                        Percentage = deduction.Percentage
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving employer benefit deductions: {ex.Message}", ex);
            }
        }

        public List<EmployerBenefitDeductionDto> GetByReport(int reportId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT 
                        Id,
                        ReportId,
                        EmployeeId,
                        CedulaJuridicaEmpresa,
                        BenefitName,
                        BenefitId,
                        DeductionAmount,
                        BenefitType,
                        Percentage,
                        CreatedDate
                    FROM EmployerBenefitDeductions
                    WHERE ReportId = @ReportId
                    ORDER BY EmployeeId, BenefitName";

                return connection.Query<EmployerBenefitDeductionDto>(query, new { ReportId = reportId }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employer benefit deductions by report: {ex.Message}");
                return new List<EmployerBenefitDeductionDto>();
            }
        }

        public List<EmployerBenefitDeductionDto> GetByEmployee(int employeeId, long cedulaJuridicaEmpresa)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT 
                        Id,
                        ReportId,
                        EmployeeId,
                        CedulaJuridicaEmpresa,
                        BenefitName,
                        BenefitId,
                        DeductionAmount,
                        BenefitType,
                        Percentage,
                        CreatedDate
                    FROM EmployerBenefitDeductions
                    WHERE EmployeeId = @EmployeeId 
                      AND CedulaJuridicaEmpresa = @CedulaJuridicaEmpresa
                    ORDER BY CreatedDate DESC";

                return connection.Query<EmployerBenefitDeductionDto>(query, new 
                { 
                    EmployeeId = employeeId,
                    CedulaJuridicaEmpresa = cedulaJuridicaEmpresa 
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employer benefit deductions by employee: {ex.Message}");
                return new List<EmployerBenefitDeductionDto>();
            }
        }
    }
}