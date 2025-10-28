namespace backend.Models
{
    public class EmployeeBenefit
    {
        public int EmployeeId { get; set; }
        public int BenefitId { get; set; }
        public char? PensionType { get; set; }
        public int? DependentsCount { get; set; }

        public string? ApiName { get; set; }
        public decimal? BenefitValue { get; set; }
        public string? BenefitType { get; set; }
    }

}
