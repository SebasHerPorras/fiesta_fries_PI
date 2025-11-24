namespace backend.Models
{
    public class EmployeeDeletionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DeletionType DeletionType { get; set; }
        public EmployeePayrollStatus PayrollStatus { get; set; } = new();
        public DateTime DeletedAt { get; set; }
    }

    public enum DeletionType
    {
        Physical,
        Logical
    }

    public class EmployeePayrollStatus
    {
        public bool HasPayments { get; set; }
        public int PaymentCount { get; set; }
        public DateTime? FirstPaymentDate { get; set; }
        public DateTime? LastPaymentDate { get; set; }
    }
}