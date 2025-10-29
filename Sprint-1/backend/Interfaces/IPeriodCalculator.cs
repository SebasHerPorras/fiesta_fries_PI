
using backend.Models.Payroll;

namespace backend.Interfaces.Strategies
{
    public interface IPeriodCalculator
    {
        bool CanHandle(string frequency);
        PayrollPeriod CalculatePeriod(DateTime baseDate, int paymentDay);
        DateTime CalculateNextPaymentDate(DateTime fromDate, int paymentDay);
    }
}