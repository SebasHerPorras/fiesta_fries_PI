namespace backend.Models.Payroll.Results
{
    public class PreviewPayrollData
    {
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalEmployeeDeductions { get; set; }
        public decimal TotalEmployerDeductions { get; set; }
        public decimal TotalBenefits { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalEmployerCost { get; set; }
    }
}