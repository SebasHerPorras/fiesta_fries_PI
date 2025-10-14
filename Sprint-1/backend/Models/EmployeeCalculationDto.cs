namespace backend.Models
{
    public class EmployeeCalculationDto
    {
        public string NombreEmpleado { get; set; } = string.Empty;
        public decimal SalarioBruto { get; set; }
        public string TipoEmpleado { get; set; } = string.Empty; // "Tiempo completo", "Medio tiempo", "Por horas", etc.
    }
}