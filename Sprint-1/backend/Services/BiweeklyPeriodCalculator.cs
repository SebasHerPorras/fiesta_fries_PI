using backend.Interfaces.Strategies;
using backend.Models;
using backend.Models.Payroll;

namespace backend.Services.Strategies
{
    public class BiweeklyPeriodCalculator : IPeriodCalculator
    {
        public bool CanHandle(string frequency) =>
            frequency?.ToLower() == "quincenal";

        public PayrollPeriod CalculatePeriod(DateTime baseDate, int paymentDay)
        {
            var nextPaymentDate = CalculateNextPaymentDate(baseDate, paymentDay);
            var period = CalculatePeriodDates(nextPaymentDate);

            return CreatePayrollPeriod(period.StartDate, period.EndDate);
        }

        public DateTime CalculateNextPaymentDate(DateTime fromDate, int paymentDay)
        {
            var currentDate = fromDate.Date;

            for (int i = 0; i < 3; i++)
            {
                var searchDate = currentDate.AddMonths(i);
                var paymentDates = GetBiweeklyPaymentDates(searchDate.Year, searchDate.Month, paymentDay);

                var nextPayment = paymentDates.FirstOrDefault(date => date > currentDate);
                if (nextPayment != default(DateTime))
                    return nextPayment;
            }

            return CalculateFirstBiweeklyDate(currentDate, paymentDay);
        }

        private List<DateTime> GetBiweeklyPaymentDates(int year, int month, int paymentDay)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var dates = new List<DateTime>();
            var firstQuincenaDay = Math.Min(15, daysInMonth); 
            dates.Add(new DateTime(year, month, firstQuincenaDay));

            var secondQuincenaDay = Math.Min(paymentDay, daysInMonth);
            if (secondQuincenaDay > 15)
            {
                dates.Add(new DateTime(year, month, secondQuincenaDay));
            }

            return dates.OrderBy(d => d).ToList();
        }

        private DateTime CalculateFirstBiweeklyDate(DateTime currentDate, int paymentDay)
        {
            var today = currentDate;

            if (today.Day <= 15)
            {
                var day15 = new DateTime(today.Year, today.Month, 15);
                if (day15 >= today) return day15;
            }

            var paymentDate = new DateTime(today.Year, today.Month,
                Math.Min(paymentDay, DateTime.DaysInMonth(today.Year, today.Month)));

            if (paymentDate > today) return paymentDate;

            return CalculateFirstBiweeklyDate(today.AddMonths(1), paymentDay);
        }

        private (DateTime StartDate, DateTime EndDate) CalculatePeriodDates(DateTime paymentDate)
        {
            DateTime startDate, endDate;

            if (paymentDate.Day <= 15)
            {
                startDate = new DateTime(paymentDate.Year, paymentDate.Month, 1);
                endDate = new DateTime(paymentDate.Year, paymentDate.Month, 15);
            }
            else
            {
                startDate = new DateTime(paymentDate.Year, paymentDate.Month, 16);
                endDate = new DateTime(paymentDate.Year, paymentDate.Month,
                    DateTime.DaysInMonth(paymentDate.Year, paymentDate.Month));
            }

            return (startDate, endDate);
        }

        private PayrollPeriod CreatePayrollPeriod(DateTime startDate, DateTime endDate)
        {
            return new PayrollPeriod
            {
                StartDate = startDate,
                EndDate = endDate,
                Description = $"Quincena {startDate:dd/MM} - {endDate:dd/MM/yyyy}",
                PeriodType = backend.Models.Payroll.PayrollPeriodType.Quincenal,
                IsProcessed = false
            };
        }
    }
}