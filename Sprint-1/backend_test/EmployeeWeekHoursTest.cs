using backend.Interfaces;
using backend.Models;
using backend.Services;
using NUnit.Framework;
using System;

namespace backend_test
{
    [TestFixture]
    public class EmployeeWeekHoursTest
    {
        private EmployeeWeekTest _service;

        [SetUp]
        public void SetUp()
        {
            _service = new EmployeeWeekTest();

        }

        [Test]
        [TestCase("2025-10-20", 119180741, 40)]
        [TestCase("2025-10-27", 123456789, 35)]
        [TestCase("2025-11-03", 987654321, 23)]
        [TestCase("2025-11-10", 112233445, 50)]
        [TestCase("2025-12-01", 555555555, 0)]
        public void GetWeek_ValidData_ReturnsExpectedWeek(string startDateStr, int idEmployee, int expectedHours)
        {
            // Arrange
            var startDate = DateTime.Parse(startDateStr);

            // Act
            var result = _service.GetWeek(startDate, idEmployee);

            // Assert
            if (expectedHours == 0)
            {
                Assert.IsNull(result, "No debería existir ninguna semana para estos datos");
            }
            else
            {
                Assert.NotNull(result, "La semana debería existir");
                Assert.AreEqual(idEmployee, result!.id_employee, "El ID del empleado no coincide");
                Assert.AreEqual(startDate, result.start_date, "La fecha de inicio de semana no coincide");
                Assert.AreEqual(expectedHours, result.hours_count, "Las horas totales no coinciden");
            }
        }
    }
}

