namespace backend.Models.Payroll
{
    public class PayrollPosition
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public long CompanyId { get; set; }
        public string Position { get; set; } = string.Empty;
    }
}
