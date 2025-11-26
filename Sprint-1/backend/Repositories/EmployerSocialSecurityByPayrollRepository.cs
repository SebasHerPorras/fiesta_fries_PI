using backend.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace backend.Repositories
{
    public class EmployerSocialSecurityByPayrollRepository
    {
        private readonly string _connectionString;

        public EmployerSocialSecurityByPayrollRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext") 
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");
        }

        public void InsertDeductions(List<EmployerSocialSecurityByPayrollDto> deductions)
        {
            if (deductions == null || !deductions.Any())
                throw new ArgumentException("La lista de deducciones no puede estar vacía");

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();


                // Usar FOR para insertar una por una
                for (int i = 0; i < deductions.Count; i++)
                {
                    var deduction = deductions[i];
                    

                    var parameters = new DynamicParameters();
                    parameters.Add("@ReportId", deduction.ReportId);
                    parameters.Add("@EmployeeId", deduction.EmployeeId);
                    parameters.Add("@ChargeName", deduction.ChargeName);
                    parameters.Add("@Amount", deduction.Amount);
                    parameters.Add("@Percentage", deduction.Percentage);
                    parameters.Add("@CedulaJuridicaEmpresa", deduction.CedulaJuridicaEmpresa);

                    connection.Execute("SP_InsertEmployerSocialSecurityByPayroll", parameters, commandType: CommandType.StoredProcedure);
                }
                
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    Console.WriteLine($" Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
    }
}