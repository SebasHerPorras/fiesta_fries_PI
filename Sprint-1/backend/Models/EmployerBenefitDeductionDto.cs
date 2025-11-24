namespace backend.Models
{
    public class EmployerBenefitDeductionDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int EmployeeId { get; set; }
        public long CedulaJuridicaEmpresa { get; set; }
        public string BenefitName { get; set; } = string.Empty;
        public int BenefitId { get; set; }
        public decimal DeductionAmount { get; set; }
        public string BenefitType { get; set; } = string.Empty; // "Fixed", "Percentage", "API"
        public decimal? Percentage { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}