using System.Reflection.Metadata;

namespace backend.Models

{
    public class PersonModel
    {
        public Guid uniqueUser { get; set; }
        public int id { get; set; }
        public string firstName { get; set; } = string.Empty;
        public string secondName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string personalPhone { get; set; } = string.Empty;
        public string? homePhone { get; set; }
        public DateTime birthdate {  get; set; }
        public string personType { get; set; } = string.Empty;
        public string direction { get; set; } = string.Empty;
    }
}
