using System;
using System.Collections.Generic;
using System.Linq;
using backend.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;

namespace backend.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository()
        {
            var builder = WebApplication.CreateBuilder();
            _connectionString = builder.Configuration.GetConnectionString("UserContext")
                ?? throw new InvalidOperationException("Connection string 'UserContext' not found. Añade ConnectionStrings:UserContext en appsettings.json.");
        }

        public List<UserModel> GetUsers()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("Connection string 'UserContext' está vacía.");

            using var connection = new SqlConnection(_connectionString);
            const string query = "SELECT PK_User AS Id, email AS Email, [password] AS PasswordHash, active, admin FROM dbo.[User]";
            return connection.Query<UserModel>(query).ToList();
        }

        public UserModel? GetById(Guid id)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = "SELECT PK_User AS Id, email AS Email, [password] AS PasswordHash, active, admin FROM dbo.[User] WHERE PK_User = @Id";
            return connection.QuerySingleOrDefault<UserModel>(query, new { Id = id });
        }

        // metodo para validar usuario por email en el login de la aplicacion
        public UserModel? GetByEmail(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = "SELECT PK_User AS Id, email AS Email, [password] AS PasswordHash, active, admin FROM dbo.[User] WHERE email = @Email";
            return connection.QuerySingleOrDefault<UserModel>(query, new { Email = email });
        }

        // Nuevo: insertar usuario (usa los mismos nombres de propiedades que UserModel)
        public void Insert(UserModel user)
        {
            using var connection = new SqlConnection(_connectionString);
            const string query = @"INSERT INTO [Fiesta_Fries_DB].[User] (PK_User, email, [password], active, admin) VALUES (@Id, @Email, @PasswordHash, @active, @admin)";
            connection.Execute(query, user);
        }

        public string get_connectionString()
        {
            return this._connectionString;
        }

        public UserModel? EmailVerification(string email_)
        {
            using var connection = new SqlConnection(this._connectionString);
            const string query = @"SELECT PK_User AS Id, email AS Email, [password] AS PasswordHash, active, admin FROM dbo.[User] WHERE email = @Email";
            return connection.QueryFirstOrDefault<UserModel>(query, new { Email = email_ });
        }
    }
}