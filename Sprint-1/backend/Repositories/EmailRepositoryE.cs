using backend.Models;
using Microsoft.Data.SqlClient;
using Dapper;

namespace backend.Repositories
{
    public class EmailRepositoryE
    {
        private readonly string _connectionString;

        public EmailRepositoryE()
        {
            var builder = WebApplication.CreateBuilder();
            this._connectionString = builder.Configuration.GetConnectionString("UserContext") ?? throw new InvalidOperationException("Ocurrió un error con el appsettings.json");
        }

        public void insertMailNoty(EmailModelE model)
        {
            //vamos a realizar el query
            using var connection = new SqlConnection(this._connectionString);
            const string query = @"INSERT INTO [Fiesta_Fries_DB].EmailVerificationE(token,expirationDate) VALUES
                                    (@token,@expirationDate)";
            connection.Execute(query, model);
        }

        public EmailModelE? getByToken(string token_)
        {
            Console.WriteLine($"Token recibido: {token_}");
            using var connection = new SqlConnection(this._connectionString);
            const string query = @"SELECT* FROM [Fiesta_Fries_DB].[EmailVerificationE] WHERE token = @token";
            return connection.QuerySingleOrDefault<EmailModelE>(query, new { token = token_ });

        }
    }
}

