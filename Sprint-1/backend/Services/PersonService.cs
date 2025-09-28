using System;
using System.Collections.Generic;
using backend.Models;
using backend.Repositories;

namespace backend.Services
{
    public class PersonService
    {
        private readonly PersonRepository repository;

            public PersonService()
        {
            this.repository = new PersonRepository();

        }
        public List<PersonModel> GetUsers()
        {
            return repository.GetAll();
        }

        public void Insert(PersonModel person)
        {
            this.repository.Insert(person);
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

    }
}
