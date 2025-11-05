using backend.Models;
using System.Data.SqlClient;
using Dapper;

namespace backend.Repositories
{
    public class EmployeeSocialSecurityContributionsRepository
    {
        private readonly string _connectionString;

        public EmployeeSocialSecurityContributionsRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext") 
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");
        }

        public List<EmployeeSocialSecurityContributions> GetActiveContributions()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT Id, [Name], [Percentage], [Active], CreationDate, ModificationDate 
                    FROM EmployeeSocialSecurityContributions 
                    WHERE [Active] = 1
                    ORDER BY [Name]";

                return connection.Query<EmployeeSocialSecurityContributions>(query).ToList();
            }
            catch (Exception ex)
            {
                return new List<EmployeeSocialSecurityContributions>();
            }
        }

        public List<EmployeeSocialSecurityContributions> GetAll()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT Id, [Name], [Percentage], [Active], CreationDate, ModificationDate 
                    FROM EmployeeSocialSecurityContributions 
                    ORDER BY [Name]";

                return connection.Query<EmployeeSocialSecurityContributions>(query).ToList();
            }
            catch (Exception ex)
            {
                return new List<EmployeeSocialSecurityContributions>();
            }
        }
    }
}