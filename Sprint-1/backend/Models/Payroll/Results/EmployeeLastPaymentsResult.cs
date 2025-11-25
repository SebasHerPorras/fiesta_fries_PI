namespace backend.Models.Payroll.Results
{
    public class EmployeeLastPaymentsResult
    {
        public int ReportId { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal SalarioBruto { get; set; }
        public decimal SalarioNeto { get; set; }
    }
}