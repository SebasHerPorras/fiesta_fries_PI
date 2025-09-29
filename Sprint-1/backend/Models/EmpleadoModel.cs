namespace backend.Models
{
    public class EmpleadoModel
    {
        public int id { get; set; } // debe ser igual al id de Persona (FK)
        public string position { get; set; } = string.Empty;
        public string employmentType { get; set; } = string.Empty;
    }
}