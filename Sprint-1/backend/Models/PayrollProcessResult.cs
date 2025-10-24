namespace backend.Models.Payroll.Results
{
    public class PayrollProcessResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? PayrollId { get; set; }
        public decimal TotalAmount { get; set; }
        public int ProcessedEmployees { get; set; }
        public Payroll? ExistingPayroll { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal TotalBenefits { get; set; }
        public decimal TotalTax { get; set; }
        public List<PayrollPayment>? PayrollDetails { get; set; }
    }
}