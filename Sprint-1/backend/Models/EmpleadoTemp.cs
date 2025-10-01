namespace backend.Models
{
    public class EmpleadoTemp
    {
        public string position { get; set; } = string.Empty;
        public string employmentType { get; set; } = string.Empty;

        public int salary { get; set; }

        public DateTime hireDate { get; set; }

        public string departament { get; set; }
        public long idCompny { get; set; }

        public string token { get; set; }

    }
}
