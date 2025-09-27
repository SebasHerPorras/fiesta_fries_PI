using System;

namespace backend.Models
{
    public class UserModel
    {
        // Coincide con UNIQUEIDENTIFIER (NEWID()) en la base de datos
        public Guid Id { get; set; }

        // nvarchar(50) NOT NULL en la tabla
        public string Email { get; set; } = string.Empty;

        // En la base de datos el campo es [password]; aquí se guarda el hash.
        // NO almacenar contraseñas en texto plano: usa PasswordHash y genera el hash antes de guardar.
        public string PasswordHash { get; set; } = string.Empty;
        public int active { get; set; } = 0;
    }
}
