namespace backend.Models.Payroll.Requests
{
    public class PayrollProcessRequest
    {
        public DateTime PeriodDate { get; set; }
        public long CompanyId { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
    }
}