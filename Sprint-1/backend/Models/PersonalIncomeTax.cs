namespace backend.Models
{
    public class PersonalIncomeTax
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public decimal Percentage { get; set; }
        public decimal BaseAmount { get; set; }
        public bool Active { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
    }
}