using System;
using System.Collections.Generic;
using System.Linq;
using backend.Models;
using Dapper;
using Microsoft.Data.SqlClient;
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
            this._connectionString = builder.Configuration.GetConnectionString("UserContext") ?? throw new InvalidOperationException("Ocurri� un error con el appsettings.json");
        }

        public void Insert(PersonModel person)
        {
            Console.WriteLine("Entra en el Insert\n");
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"INSERT INTO [Fiesta_Fries_DB].[Persona](id,firstName,secondName,birthdate,direction,personalPhone,homePhone,uniqueUser,personType) VALUES
                                 (@id, @firstName, @secondName,@birthdate,@direction,@personalPhone,@homePhone,@uniqueUser,@personType)";

            Console.WriteLine("Query realizado con �xito");
            connection.Execute(query, person);
                                  
        }

        public List<PersonModel> GetAll()
        {
            using var connection = new SqlConnection(this._connectionString);
            const string query = "SELECT * from [Fiesta_Fries_DB].[Persona]";
            var data = connection.Query<PersonModel>(query).ToList();
            return data;
        }

        public PersonModel? GetById([FromQuery]Guid id)
        {
            using var connection = new SqlConnection(this._connectionString);
            const string query = "SELECT * FROM [Fiesta_Fries_DB].[Persona] WHERE uniqueUser = @uniqueUser";
            Console.WriteLine("Query funciona con �xito\n");
            return connection.QuerySingleOrDefault<PersonModel>(query, new { uniqueUser = id });

        }

        public PersonModel? GetByIdentity(int id_)
        {
            using var connection = new SqlConnection(this._connectionString);

            const string query = @"SELECT* FROM [Fiesta_Fries_DB].[PERSONA] WHERE id = @id";
            Console.WriteLine("Querry realizado con �xito\n");

            return connection.QuerySingleOrDefault<PersonModel>(query, new { id = id_ });
        }
        public PersonModel? GetByUserId(Guid usuarioId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = @"
                SELECT p.* 
                FROM [Fiesta_Fries_DB].[Persona] p
                INNER JOIN [Fiesta_Fries_DB].[User] u ON p.uniqueUser = u.PK_User
                WHERE p.uniqueUser = @UsuarioId
                  AND u.active = 1";

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

        public PersonalProfileDto? GetPersonalProfile(Guid usuarioId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                var query = @"
                SELECT 
                    p.firstName AS FirstName,
                    p.secondName AS SecondName,
                    p.birthdate AS Birthdate,
                    p.direction AS Direction,
                    p.personType as PersonType,
                    p.personalPhone AS PersonalPhone,
                    p.homePhone AS HomePhone,
                    u.email AS Email
                    
                FROM [Fiesta_Fries_DB].[Persona] p INNER JOIN [Fiesta_Fries_DB].[User] u
                    ON p.uniqueUser = u.PK_User
                WHERE p.uniqueUser = @UsuarioId";

                return connection.QueryFirstOrDefault<PersonalProfileDto>(query, new { UsuarioId = usuarioId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetPersonalProfile: {ex.Message}");
                return null;
            }
        }

        public void Update(PersonModel persona)
        {
            using var connection = new SqlConnection(_connectionString);

            const string query = @"
                UPDATE [Fiesta_Fries_DB].[Persona] SET
                    firstName = @firstName,
                    secondName = @secondName,
                    direction = @direction,
                    personalPhone = @personalPhone,
                    homePhone = @homePhone
                WHERE id = @id";

            connection.Execute(query, persona);
        }


    }
}
