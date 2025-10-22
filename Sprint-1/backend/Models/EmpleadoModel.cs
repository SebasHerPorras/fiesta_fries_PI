namespace backend.Models
{
    public class EmpleadoModel
    {
        public int id { get; set; } // debe ser igual al id de Persona (FK)
        public string position { get; set; } = string.Empty;
        public string employmentType { get; set; } = string.Empty;

        public int salary { get; set; }

        public DateTime hireDate { get; set; }

        public string department { get; set; }

        public long idCompny {  get; set; }

        public string email { get; set; } = string.Empty;

        public string name { get; set; } = string.Empty;
    }
}