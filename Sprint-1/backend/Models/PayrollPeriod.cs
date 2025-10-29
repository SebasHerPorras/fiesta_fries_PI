namespace backend.Models.Payroll
{
    public class PayrollPeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsProcessed { get; set; }
        public PayrollPeriodType PeriodType { get; set; } = PayrollPeriodType.Mensual; // Mensual, Quincenal
    }
}