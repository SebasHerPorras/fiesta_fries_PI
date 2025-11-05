using NUnit.Framework;
using System;
using backend.Interfaces.Strategies;
using backend.Models.Payroll;
using backend.Services.Strategies;

namespace backend_test
{
    [TestFixture]
    public class PeriodCalculatorTests
    {
        private MonthlyPeriodCalculator _calculator;

        [SetUp]
        public void Setup()
        {
            _calculator = new MonthlyPeriodCalculator();
        }

        [Test]
        public void CanHandle_WhenMonthly_ReturnsTrue()
        {
            // Act
            var result = _calculator.CanHandle("mensual");

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CalculatePeriod_ForFebruaryLeapYear_HandlesCorrectly()
        {
            // Arrange
            var baseDate = new DateTime(2024, 2, 15);
            var paymentDay = 29;

            // Act
            var period = _calculator.CalculatePeriod(baseDate, paymentDay);

            // Assert
            Assert.That(period, Is.Not.Null);
            // EndDate = 28 mar (29 mar - 1 día)
            Assert.That(period.StartDate, Is.EqualTo(new DateTime(2024, 2, 29)));
            Assert.That(period.EndDate, Is.EqualTo(new DateTime(2024, 3, 28)));
            Assert.That(period.Description, Is.EqualTo("Mes febrero 2024"));
            Assert.That(period.PeriodType, Is.EqualTo(PayrollPeriodType.Mensual));
        }

        [Test]
        public void CalculatePeriod_WhenPaymentDayGreaterThanMonthDays_UsesLastDay()
        {
            // Arrange - April has 30 days
            var baseDate = new DateTime(2025, 4, 15);
            var paymentDay = 31;

            // Act
            var period = _calculator.CalculatePeriod(baseDate, paymentDay);

            // Assert
            Assert.That(period, Is.Not.Null);
            // EndDate = 29 may (30 may - 1 día)
            Assert.That(period.StartDate, Is.EqualTo(new DateTime(2025, 4, 30)));
            Assert.That(period.EndDate, Is.EqualTo(new DateTime(2025, 5, 29)));
            Assert.That(period.Description, Is.EqualTo("Mes abril 2025"));
            Assert.That(period.PeriodType, Is.EqualTo(PayrollPeriodType.Mensual));
        }

        [Test]
        public void CalculateNextPaymentDate_HandlesYearTransition()
        {
            // Arrange
            var fromDate = new DateTime(2025, 12, 15);
            var paymentDay = 31;

            // Act
            var nextPaymentDate = _calculator.CalculateNextPaymentDate(fromDate, paymentDay);

            // Assert
            Assert.That(nextPaymentDate, Is.EqualTo(new DateTime(2025, 12, 31)));
            
            var januaryPayment = _calculator.CalculateNextPaymentDate(
                new DateTime(2026, 1, 1),
                paymentDay
            );
            Assert.That(januaryPayment, Is.EqualTo(new DateTime(2026, 1, 31)));
        }

        [Test]
        public void CalculatePeriod_CreatesValidDescription()
        {
            // Arrange
            var baseDate = new DateTime(2024, 3, 15);
            var paymentDay = 15;

            // Act
            var period = _calculator.CalculatePeriod(baseDate, paymentDay);

            // Assert
            Assert.That(period, Is.Not.Null);
            Assert.That(period.Description, Is.EqualTo("Mes marzo 2024"));
        }

        [Test]
        public void CalculateNextPaymentDate_ForCurrentMonth_ReturnsCorrectDate()
        {
            // Arrange
            var fromDate = new DateTime(2024, 6, 10);
            var paymentDay = 15;

            // Act
            var nextPaymentDate = _calculator.CalculateNextPaymentDate(fromDate, paymentDay);

            // Assert
            Assert.That(nextPaymentDate, Is.EqualTo(new DateTime(2024, 6, 15)));
        }

        [Test]
        public void CalculateNextPaymentDate_AfterPaymentDay_ReturnsNextMonth()
        {
            // Arrange
            var fromDate = new DateTime(2024, 6, 20);
            var paymentDay = 15;

            // Act
            var nextPaymentDate = _calculator.CalculateNextPaymentDate(fromDate, paymentDay);

            // Assert
            Assert.That(nextPaymentDate, Is.EqualTo(new DateTime(2024, 7, 15)));
        }

        [Test]
        public void CalculatePeriod_WithNormalCase_ReturnsExpectedPeriod()
        {
            // Arrange - Caso normal
            var baseDate = new DateTime(2024, 5, 10);
            var paymentDay = 20;

            // Act
            var period = _calculator.CalculatePeriod(baseDate, paymentDay);

            // Assert
            Assert.That(period, Is.Not.Null);
            Assert.That(period.StartDate, Is.EqualTo(new DateTime(2024, 5, 20)));
            Assert.That(period.EndDate, Is.EqualTo(new DateTime(2024, 6, 19)));
            Assert.That(period.Description, Is.EqualTo("Mes mayo 2024"));
        }

        [Test]
        public void CalculatePeriod_EndOfMonth_CorrectEndDateCalculation()
        {
            // Arrange
            var baseDate = new DateTime(2024, 1, 31);
            var paymentDay = 31;

            // Act
            var period = _calculator.CalculatePeriod(baseDate, paymentDay);

            // Assert
            Assert.That(period, Is.Not.Null);
            // EndDate = 28 feb (29 feb - 1 día)
            Assert.That(period.StartDate, Is.EqualTo(new DateTime(2024, 1, 31)));
            Assert.That(period.EndDate, Is.EqualTo(new DateTime(2024, 2, 28)));
        }
    }
}