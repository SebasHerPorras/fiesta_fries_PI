namespace backend.Models.Payroll
{
    public class EmployeeHistoricalReportDto
    {
        public string EmploymentType { get; set; }
        public string Position { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Salary { get; set; }
        public decimal DeductionsAmount { get; set; }
        public decimal BenefitsAmount { get; set; }
        public decimal NetSalary { get; set; }
    }
}
