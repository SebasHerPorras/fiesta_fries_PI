using backend.Interfaces.Strategies;
using backend.Models;
using backend.Models.Payroll;

namespace backend.Services.Strategies
{
    public class MonthlyPeriodCalculator : IPeriodCalculator
    {
        public bool CanHandle(string frequency) =>
            frequency?.ToLower() == "mensual";

        public PayrollPeriod CalculatePeriod(DateTime baseDate, int paymentDay)
        {
            var nextPaymentDate = CalculateNextPaymentDate(baseDate, paymentDay);
            var periodStartDate = nextPaymentDate.AddMonths(-1).AddDays(1);
            var periodEndDate = nextPaymentDate;

            return CreatePayrollPeriod(periodStartDate, periodEndDate);
        }

        public DateTime CalculateNextPaymentDate(DateTime fromDate, int paymentDay)
        {
            var currentDate = fromDate.Date;
            DateTime nextPaymentDate;

            for (int i = 0; i < 6; i++)
            {
                var searchDate = currentDate.AddMonths(i);
                var paymentDate = GetMonthlyPaymentDate(searchDate.Year, searchDate.Month, paymentDay);

                if (paymentDate >= currentDate)
                    return paymentDate;
            }

            throw new InvalidOperationException("No se pudo determinar la próxima fecha de pago mensual");
        }

        private DateTime GetMonthlyPaymentDate(int year, int month, int paymentDay)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var actualPaymentDay = Math.Min(paymentDay, daysInMonth);

            return new DateTime(year, month, actualPaymentDay);
        }


        private PayrollPeriod CreatePayrollPeriod(DateTime startDate, DateTime endDate)
        {
            return new PayrollPeriod
            {
                StartDate = startDate,
                EndDate = endDate, // Día de pago
                Description = $"Periodo {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}",
                PeriodType = backend.Models.Payroll.PayrollPeriodType.Mensual
            };
        }
    }
}