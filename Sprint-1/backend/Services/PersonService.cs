using System;
using System.Collections.Generic;
using backend.Models;
using backend.Repositories;

namespace backend.Services
{
    public class PersonService
    {
        private readonly PersonRepository repository;
        private readonly UserRepository userRepository;

        public PersonService()
        {
            this.repository = new PersonRepository();
            this.userRepository = new UserRepository();
        }

        public List<PersonModel> GetUsers()
        {
            return repository.GetAll();
        }

        // Ahora devuelve la persona creada (y crea User si hace falta)
        public PersonModel Insert(PersonModel person)
        {
            if (person == null) throw new ArgumentNullException(nameof(person));

            // Si uniqueUser no viene asignado, crear user asociado
            if (person.uniqueUser == Guid.Empty)
            {
                var newUser = new UserModel
                {
                    Id = Guid.NewGuid(),
                    Email = person.email?.Trim() ?? string.Empty,
                    PasswordHash = Guid.NewGuid().ToString(), // temporal: si el front no envía password; en producción hashea/usa password real
                    active = 0
                };

                // prevenir duplicados por email: si existe, usarlo
                var existing = userRepository.GetByEmail(newUser.Email);
                if (existing != null)
                {
                    newUser.Id = existing.Id;
                }
                else
                {
                    userRepository.Insert(newUser);
                }

                person.uniqueUser = newUser.Id;
            }

            // Insertar Persona (repository se encarga del INSERT)
            repository.Insert(person);

            // Devolver la persona (el caller recibe el id y demás campos)
            return person;
        }

        public PersonModel? GetById(Guid id)
        {
            if (id == Guid.Empty) return null;
            return repository.GetById(id);
        }

        public PersonModel? GetByUserId(Guid usuarioId)
        {
            try
            {
                return repository.GetByUserId(usuarioId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR:Buscando persona por usuario: {ex.Message}");
                return null;
            }
        }

        //Demás cositas...
    }
}
