using backend.Interfaces;
using backend.Models;
using backend.Services;
using backend.Tests;
using NUnit.Framework;
using System;

namespace backend.Tests
{
    [TestFixture]
    public class EmployeeDayServiceTests
    {
        private IEmployeeWorkDayService _service;

        [SetUp]
        public void Setup()
        {
            _service = new EmployeeDayTest();
        }

        [Test]
        [TestCase("2025-10-20", "2025-10-22", 119180745, 4, 2, 6)]
        [TestCase("2025-10-21", "2025-10-23", 123456789, 1, 3, 4)]
        public void AddHours_WhenDayExists_ShouldAccumulateHours(string weekStartStr, string dayStr, int idEmployee, int firstHours, int secondHours, int expectedTotal)
        {
            // Arrange
            var weekStart = DateTime.Parse(weekStartStr);
            var day = DateTime.Parse(dayStr);

            // Act
            _service.AddHours(weekStart, day, firstHours, idEmployee);
            var result = _service.AddHours(weekStart, day, secondHours, idEmployee);

            // Assert
            Assert.NotNull(result, "El resultado no debe ser nulo");
            Assert.AreEqual(expectedTotal, result!.hours_count, "Las horas no se acumularon correctamente");
            Assert.AreEqual(idEmployee, result.id_employee, "El ID del empleado no coincide");
            Assert.AreEqual(day, result.date, "La fecha del día no coincide");
        }

        [Test]
        [TestCase("2025-10-20", "2025-10-23", 119180745, 5)]
        [TestCase("2025-10-21", "2025-10-24", 123456789, 7)]
        public void AddHours_WhenDayNotExists_ShouldCreateNew(string weekStartStr, string dayStr, int idEmployee, int hours)
        {
            // Arrange
            var weekStart = DateTime.Parse(weekStartStr);
            var day = DateTime.Parse(dayStr);

            // Act
            var result = _service.AddHours(weekStart, day, hours, idEmployee);

            // Assert
            Assert.NotNull(result, "El resultado no debe ser nulo");
            Assert.AreEqual(hours, result!.hours_count, "Las horas no se asignaron correctamente");
            Assert.AreEqual(idEmployee, result.id_employee, "El ID del empleado no coincide");
            Assert.AreEqual(day, result.date, "La fecha del día no coincide");
        }

        [Test]
        [TestCase("2025-10-20", "2025-10-22", 119180745, 3)]
        [TestCase("2025-10-21", "2025-10-23", 123456789, 5)]
        public void GetWorkDay_WhenExists_ShouldReturnCorrectModel(string weekStartStr, string dayStr, int idEmployee, int hours)
        {
            // Arrange
            var weekStart = DateTime.Parse(weekStartStr);
            var day = DateTime.Parse(dayStr);
            _service.AddHours(weekStart, day, hours, idEmployee);

            // Act
            var result = _service.GetWorkDay(weekStart, day, idEmployee);

            // Assert
            Assert.NotNull(result, "El resultado no debe ser nulo");
            Assert.AreEqual(hours, result!.hours_count, "Las horas no coinciden");
            Assert.AreEqual(idEmployee, result.id_employee, "El ID del empleado no coincide");
            Assert.AreEqual(day, result.date, "La fecha del día no coincide");
        }

        [Test]
        [TestCase("2025-10-20", "2025-10-24", 119180745)]
        [TestCase("2025-10-21", "2025-10-25", 123456789)]
        public void GetWorkDay_WhenNotExists_ShouldReturnNull(string weekStartStr, string dayStr, int idEmployee)
        {
            // Arrange
            var weekStart = DateTime.Parse(weekStartStr);
            var day = DateTime.Parse(dayStr);

            // Act
            var result = _service.GetWorkDay(weekStart, day, idEmployee);

            // Assert
            Assert.IsNull(result, "El resultado debe ser nulo porque no existe el día");
        }
    }
}


