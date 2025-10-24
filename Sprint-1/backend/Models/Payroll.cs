namespace backend.Models.Payroll
{
    public class Payroll
    {
        public int PayrollId { get; set; }
        public DateTime PeriodDate { get; set; }
        public long CompanyId { get; set; }
        public bool IsCalculated { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime LastModified { get; set; }
    }
}