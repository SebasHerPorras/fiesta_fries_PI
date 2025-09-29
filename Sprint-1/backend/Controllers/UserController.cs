﻿using backend.Models;
using backend.Repositories;
using backend.Services;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Net.Quic;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController()
        {
            userService = new UserService();
        }

        [HttpGet]
        public List<UserModel> Get()
        {
            var users = userService.GetUsers();
            return users;
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email y password son requeridos.");

            // DEBUG seguro: email y longitud de password (no imprimir la contraseña)
            Console.WriteLine($"[DEBUG] Incoming login request. Email: '{request.Email}' PasswordLength: {request.Password?.Length ?? 0}");

            var user = userService.Authenticate(request.Email.Trim(), request.Password);
            if (user == null)
            {
                Console.WriteLine("[DEBUG] Authentication failed for email: " + request.Email);
                return Unauthorized("Credenciales inválidas.");
            }

            Console.WriteLine("[DEBUG] Authentication succeeded for user id: " + user.Id);

            var personaService = new PersonService();
            var persona = personaService.GetByUserId(user.Id);

            if (persona == null)
            {
                Console.WriteLine("[DEBUG] Usuario no tiene persona asociada: " + user.Id);
                return BadRequest("El usuario no tiene una persona asociada.");
            }

            Console.WriteLine($"[DEBUG] Persona encontrada: ID {persona.id}, Tipo: {persona.personType}");

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                personaId = persona.id,
                personType = persona.personType,
                firstName = persona.firstName,
                secondName = persona.secondName
            });
        }

        [HttpPost("create")]
        public ActionResult Create([FromBody] UserModel request)
        {
            Console.WriteLine("Entro en el método de creación de usuario\n");
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.PasswordHash)) {
                return BadRequest("Email y password son requeridos");
            }

            var newUser = new UserModel
            {
                Id = Guid.NewGuid(),
                Email = request.Email.Trim(),
                PasswordHash = request.PasswordHash.Trim(),
            };

            var tempRepo = new UserRepository();
            string connectionString = tempRepo.get_connectionString();

            using var connection = new SqlConnection(connectionString);

            const string query = @"INSERT INTO dbo.[User] (PK_User , email, password) VALUES (@Id, @Email, @PasswordHash)";
            connection.Execute(query, newUser);

            Console.WriteLine("Query realizado con éxito\n");




            //Ojito pq aquí vamos a añadir la vara del correo electrónico 

            // vamos a generar el token 

            var token = Guid.NewGuid().ToString();

            // La expiración va a ser cada 3 días

            DateTime expiration = DateTime.Now.AddDays(3);

            var mailTokenVerification = new MailModel
            {
                userID = newUser.Id,
                token = token,
                experationDate = expiration
            };


            //Justo aquí tengo que conectarme para modificar la tabla, realzar un insert
            var mailRepository = new MailRepository();



            mailRepository.insertMailNoty(mailTokenVerification);

            Console.WriteLine("Query realizado con éxito\n");

            //preparamos el link al api donde vamos a manejar la vaina
            var verificationLink = $"http://localhost:5081/api/user/verify?token={token}";

            // vamo a enviar el correo
            var mailAddr = new MailAddress("pruebadisenosoft@gmail.com", "Fiesta Fries");

            var sendTo = new MailAddress(newUser.Email);

            const string password = "rxhd qmzc uvxi sxmg";

            const string subject = "Verificación de Creación de usuario Fiesta Fries";

            string body = $"¡Saludos cordiales!, somos la gente de Fiesta Fries enviandote el enlace de verficación de usuario para finalizar el proceso de creación de tu cuenta: {verificationLink}";

            // vamo a crear una instancia al servicio que nos va a facilitar todas las cosas
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                //Estos son la credenciales que generamos anteriormente 
                Credentials = new NetworkCredential(mailAddr.Address, password)
            };

            //Aquí vamos a enviar la vaina
            //Le indicamos que vamos a enviar 
            using (var message = new MailMessage(mailAddr, sendTo)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }

            return Ok(new { id = newUser.Id, email = newUser.Email });
        }

        //Api para la validación de usuari
        [HttpGet("verify")]
        public ActionResult verification([FromQuery] string token)
        {
            MailRepository repo = new MailRepository();

            var verificationMail = repo.getByToken(token);

            /// Aquí hacemos las validaciones para el token
            /// 
            if (verificationMail == null || verificationMail.experationDate < DateTime.UtcNow)
            {
                return BadRequest("El token es nulo o ya caducó");
            }

            /// Necesito verificarlo
            /// objeto para realizar la verificación
            var verificationDate = new
            {
                active = 1,
                userFK = verificationMail.userID
            };

            UserRepository repos = new UserRepository();

            //onst string connection_string = repos.

            var connection = new SqlConnection(repos.get_connectionString());


            const string query = @"UPDATE dbo.[User] SET active = @active WHERE PK_User = @userFk";

            connection.Execute(query, verificationDate);

            return Ok("Correo verificado con éxito");
        }

    }

}
