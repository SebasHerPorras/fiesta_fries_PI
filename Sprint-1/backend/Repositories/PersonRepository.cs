using System;
using System.Collections.Generic;
using System.Linq;
using backend.Models;
using Dapper;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;


namespace backend.Repositories
{
    public class PersonRepository
    {
        private readonly string _connectionString;

        public PersonRepository()
        {
            var builder = WebApplication.CreateBuilder();
            this._connectionString = builder.Configuration.GetConnectionString("UserContext") ?? throw new InvalidOperationException("Ocurrió un error con el appsettings.json");
        }

        public void Insert(PersonModel person)
        {
            Console.WriteLine("Entra en el Insert\n");
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"INSERT INTO dbo.Persona(id,firstName,secondName,birthdate,direction,personalPhone,homePhone,uniqueUser,personType) VALUES
                                 (@id, @firstName, @secondName,@birthdate,@direction,@personalPhone,@homePhone,@uniqueUser,@personType)";

            Console.WriteLine("Query realizado con éxito");
            connection.Execute(query, person);
                                  
        }

        public List<PersonModel> GetAll()
        {
            using var connection = new SqlConnection(this._connectionString);
            const string query = "SELECT * from dbo.Persona";
            var data = connection.Query<PersonModel>(query).ToList();
            return data;
        }

        public PersonModel? GetById([FromQuery]Guid id)
        {
            using var connection = new SqlConnection(this._connectionString);
            const string query = "SELECT * FROM dbo.Persona WHERE uniqueUser = @uniqueUser";
            Console.WriteLine("Query funciona con éxito\n");
            return connection.QuerySingleOrDefault<PersonModel>(query, new { uniqueUser = id });

        }

        public PersonModel? GetByIdentity(int id_)
        {
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"SELECT* FROM PERSONA WHERE id = @id";
            Console.WriteLine("Querry realizado con éxito\n");

            return connection.QuerySingleOrDefault<PersonModel>(query, new { id = id_ });
        }

        public PersonModel? GetByUserId(Guid usuarioId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = @"
            SELECT * 
            FROM Persona 
            WHERE uniqueUser = @UsuarioId 
            AND active = 1";

                return connection.QueryFirstOrDefault<PersonModel>(query, new
                {
                    UsuarioId = usuarioId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] En repository GetByUserId: {ex.Message}");
                return null;
            }
        }



    }
}
