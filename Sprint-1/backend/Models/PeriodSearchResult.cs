using backend.Models.Payroll;

namespace backend.Models.Common
{
    public class PeriodSearchResult
    {
        public List<PayrollPeriod> Periods { get; set; } = new();
        public int TotalGenerated { get; set; }
        public bool SearchLimitReached { get; set; }
    }
}