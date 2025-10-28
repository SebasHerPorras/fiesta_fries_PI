public class PayrollSummaryResult
{
    public int PayrollId { get; set; }
    public DateTime PeriodDate { get; set; }
    public string CompanyId { get; set; }
    public bool IsCalculated { get; set; }
    public string ApprovedBy { get; set; }
    public DateTime LastModified { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalEmployerDeductions { get; set; }
    public decimal TotalEmployeeDeductions { get; set; }
    public decimal TotalBenefits { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalEmployerCost { get; set; }
}
