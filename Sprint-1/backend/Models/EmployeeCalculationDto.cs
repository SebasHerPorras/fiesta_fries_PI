namespace backend.Models
{
    public class EmployeeCalculationDto
    {
        public long CedulaEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public decimal SalarioBruto { get; set; }
        public string TipoEmpleado { get; set; } = string.Empty;
        public int HorasTrabajadas { get; set; }
        public DateTime Cumpleanos { get; set; }
        public int horas { get; set; }
    }
}