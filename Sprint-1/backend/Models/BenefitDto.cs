namespace backend.Models
{
    public class BenefitDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "Monto Fijo", "Porcentual", "API"
        public decimal Value { get; set; }
        public string ApiName { get; set; } = string.Empty; // Si es API: "SolidarityAssociation", "PrivateInsurance", "VoluntaryPensions"
    }
}