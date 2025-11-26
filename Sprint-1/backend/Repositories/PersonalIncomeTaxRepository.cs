using backend.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace backend.Repositories
{
    public class PersonalIncomeTaxRepository
    {
        private readonly string _connectionString;

        public PersonalIncomeTaxRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext") 
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found.");
        }

        public List<PersonalIncomeTax> GetActiveScales()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT Id, [Name], MinAmount, MaxAmount, [Percentage], BaseAmount, [Active], CreationDate, ModificationDate 
                    FROM [Fiesta_Fries_DB].[PersonalIncomeTax] WHERE [Active] = 1
                    ORDER BY MinAmount";

                return connection.Query<PersonalIncomeTax>(query).ToList();
            }
            catch (Exception ex)
            {
                return new List<PersonalIncomeTax>();
            }
        }

        public List<PersonalIncomeTax> GetAll()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT Id, [Name], MinAmount, MaxAmount, [Percentage], BaseAmount, [Active], CreationDate, ModificationDate 
                    FROM [Fiesta_Fries_DB].[PersonalIncomeTax] ORDER BY MinAmount";

                return connection.Query<PersonalIncomeTax>(query).ToList();
            }
            catch (Exception ex)
            {
                return new List<PersonalIncomeTax>();
            }
        }
    }
}