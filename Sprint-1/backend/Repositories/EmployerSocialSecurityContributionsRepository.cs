using backend.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace backend.Repositories
{
    public class EmployerSocialSecurityContributionsRepository
    {
        private readonly string _connectionString;

        public EmployerSocialSecurityContributionsRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext") 
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");
        }

        public List<EmployerSocialSecurityContributions> GetActiveContributions()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT [Name], [Percentage] 
                    FROM [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] WHERE [Active] = 1
                    ORDER BY [Name]";

                return connection.Query<EmployerSocialSecurityContributions>(query).ToList();
            }
            catch (Exception ex)
            {
                return new List<EmployerSocialSecurityContributions>();
            }
        }

        public List<EmployerSocialSecurityContributions> GetAll()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT [Name], [Percentage] 
                    FROM [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] WHERE [Active] = 1
                    ORDER BY [Name]";

                return connection.Query<EmployerSocialSecurityContributions>(query).ToList();
            }
            catch (Exception ex)
            {
               
                return new List<EmployerSocialSecurityContributions>();
            }
        }
    }
}