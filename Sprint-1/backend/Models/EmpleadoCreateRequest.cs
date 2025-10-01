using System;

namespace backend.Models
{
    // DTO que recibe el front: datos de persona + credenciales + campo de empleado
    public class EmpleadoCreateRequest
    {
        // Datos de persona (front debe enviar el id numérico)
        public int personaId { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string secondName { get; set; } = string.Empty;
        public DateTime birthdate { get; set; }
        public string direction { get; set; } = string.Empty;
        public string personalPhone { get; set; } = string.Empty;
        public string? homePhone { get; set; }
        public string personType { get; set; } = "Empleado";

        // Credenciales para crear User (si la persona no existe)
        public string userEmail { get; set; } = string.Empty;
        public string userPassword { get; set; } = string.Empty;

        // Datos de empleado
        public string position { get; set; } = string.Empty;
        public string employmentType { get; set; } = "Tiempo completo";

        public int salary { get; set; }

        public DateTime hireDate { get; set; }

        public string departament { get; set; }
        public long idCompny { get; set; }
    }
}