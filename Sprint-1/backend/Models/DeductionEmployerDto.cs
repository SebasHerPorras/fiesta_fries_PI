namespace backend.Models
{
    public class DeductionEmployerDto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Porcentaje { get; set; }
        public decimal Monto { get; set; }
    }
}