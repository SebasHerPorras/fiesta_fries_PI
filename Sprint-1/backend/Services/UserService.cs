using System;
using System.Collections.Generic;
using backend.Models;
using backend.Repositories;

namespace backend.Services
{
    public class UserService
    {
        private readonly UserRepository userRepository;

        public UserService()
        {
            userRepository = new UserRepository();
        }

        // Devuelve todos los usuarios (sin lógica adicional por ahora), esto es el template inicial
        // del laboratorio.
        public List<UserModel> GetUsers()
        {
            return userRepository.GetUsers();
        }

        // Devuelve un usuario por Id
        public UserModel? GetById(Guid id)
        {
            if (id == Guid.Empty) return null;
            return userRepository.GetById(id);
        }

        //cambia el método Authenticate para loggear correctamente y comparar con trim
        public UserModel? Authenticate(string email, string password)
        {
            // DEBUG: mostrar lo que llega (enmascarar password)
            var masked = string.IsNullOrEmpty(password) ? "(empty)" : password.Length <= 2 ? new string('*', password.Length) : password.Substring(0,1) + new string('*', password.Length-2) + password.Substring(password.Length-1);
            Console.WriteLine($"[DEBUG] Authenticate called. Email: '{email}' Password(masked): '{masked}'");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = userRepository.GetByEmail(email.Trim());
            if (user == null)
            {
                Console.WriteLine("[DEBUG] No user found for email: " + email);
                return null;
            }
            
            // DEBUG: muestra (enmascarado) lo que hay en la DB
            var dbMasked = string.IsNullOrEmpty(user.PasswordHash) ? "(empty)" : user.PasswordHash.Length <= 2 ? new string('*', user.PasswordHash.Length) : user.PasswordHash.Substring(0,1) + new string('*', user.PasswordHash.Length-2) + user.PasswordHash.Substring(user.PasswordHash.Length-1);
            Console.WriteLine($"[DEBUG] User found. Id: {user.Id} Email: {user.Email} PasswordInDb(masked): '{dbMasked}'");

            // comparar con trim para evitar espacios accidentales
            if (user.PasswordHash?.Trim() == password.Trim())
            {
                Console.WriteLine("[DEBUG] Password match OK");
                return user;
            }

            Console.WriteLine("[DEBUG] Password mismatch");
            return null;
        }

        public bool EmailConfirmation(string email)
        {
            UserModel? user = this.userRepository.EmailVerification(email);

            if (user == null)
            {
                return false;
            }

            return true;
            
        }
    }
}