namespace backend.Models
{
    public class ResultadoDeduccionesEmpleadorDto
    {
        public string NombreEmpleado { get; set; } = string.Empty;
        public decimal SalarioBruto { get; set; }
        public List<DeductionEmployerDto> DeduccionesEmpleador { get; set; } = new();
        public decimal TotalDeduccionesEmpleador { get; set; }
        public decimal PorcentajeTotalEmpleador { get; set; }
    }
}