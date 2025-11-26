using backend.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace backend.Repositories
{
    public class MailRepository
    {
        private readonly string _connectionString;

        public MailRepository()
        {
            var builder = WebApplication.CreateBuilder();
            this._connectionString = builder.Configuration.GetConnectionString("UserContext") ?? throw new InvalidOperationException("Ocurrió un error con el appsettings.json");
        }

        public void insertMailNoty(MailModel model)
        {
            //vamos a realizar el query
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"INSERT INTO EmailVerification(userID,token,experationDate) VALUES
                                   (@userID,@token,@experationDate)";
            connection.Execute(query, model);
        }

        public MailModel? getByToken(string token_)
        {

            var connection = new SqlConnection(this._connectionString);

            const string query = @"SELECT* FROM [Fiesta_Fries_DB].[EmailVerification] WHERE token = @token";

            Console.WriteLine("Query realizado con éxito\n");
            return connection.QuerySingleOrDefault<MailModel>(query,new{ token = token_});

        }
    }
}
