namespace backend.Models.Payroll
{
    public class PayrollPayment
    {
        public int PaymentId { get; set; }
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal DeductionsAmount { get; set; }
        public decimal BenefitsAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal NetSalary { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; } = "PENDIENTE";
    }
}