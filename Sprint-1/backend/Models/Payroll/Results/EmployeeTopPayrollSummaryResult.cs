public class EmployeeTopPayrollSummaryResult
{
    public int PayrollId { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public DateTime PeriodDate { get; set; }
}