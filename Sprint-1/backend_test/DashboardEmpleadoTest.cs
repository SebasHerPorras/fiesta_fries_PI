using backend.Models;
using backend.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace backend_test
{
    [TestFixture]
    public class EmployeeDashboardServiceTests
    {
        private EmployeeDashboardServiceTest _service;

        [SetUp]
        public void SetUp()
        {
            _service = new EmployeeDashboardServiceTest();
        }

        [Test]
        public void GetDashboardData_NoDeductions_ReturnsFullSalary()
        {
            int id = 10;
            DateTime date = DateTime.Parse("2025-01-01");

            _service.AddSalary(id, 1000);

            var result = _service.GetDashboardData(id, date);

            Assert.NotNull(result);
            Assert.AreEqual(1000, result!.CrudSalary);
            Assert.AreEqual(0, result.TotalDeductions);
            Assert.AreEqual(1000, result.NetSalary);
            Assert.AreEqual(0, result.ReteinedPercentage);
        }

        [Test]
        public void GetDashboardData_WithSeveralDeductions_ComputesCorrectTotals()
        {
            int id = 20;
            DateTime date = DateTime.Parse("2025-01-10");

            _service.AddSalary(id, 2000);

            _service.AddPayrollDeduction(new EmployeeDeductionsByPayrollModel
            {
                EmployeeId = id,
                DeductionAmount = 300,
                CreatedDate = date
            });

            _service.AddPayrollDeduction(new EmployeeDeductionsByPayrollModel
            {
                EmployeeId = id,
                DeductionAmount = 200,
                CreatedDate = date
            });

            var result = _service.GetDashboardData(id, date);

            Assert.NotNull(result);
            Assert.AreEqual(2000, result!.CrudSalary);
            Assert.AreEqual(500, result.TotalDeductions);
            Assert.AreEqual(1500, result.NetSalary);

            Assert.AreEqual(25, result.ReteinedPercentage);
        }

        [Test]
        public void GetDashboardData_NoSalary_HandlesGracefully()
        {
            int id = 30;
            DateTime date = DateTime.Parse("2025-02-01");

            var result = _service.GetDashboardData(id, date);

            Assert.NotNull(result);
            Assert.AreEqual(0, result!.CrudSalary);
            Assert.AreEqual(0, result.TotalDeductions);
            Assert.AreEqual(0, result.NetSalary);
            Assert.AreEqual(0, result.ReteinedPercentage);
        }

        [Test]
        public void GetEmployeePayrollData_DeductionsDifferentDate_AreIgnored()
        {
            int id = 40;

            DateTime queryDate = DateTime.Parse("2025-03-05");
            DateTime otherDate = DateTime.Parse("2025-03-10");

            _service.AddSalary(id, 1200);

            _service.AddPayrollDeduction(new EmployeeDeductionsByPayrollModel
            {
                EmployeeId = id,
                DeductionAmount = 300,
                CreatedDate = otherDate // distinta fecha
            });

            var result = _service.GetDashboardData(id, queryDate);

            Assert.NotNull(result);
            Assert.AreEqual(1200, result!.CrudSalary);
            Assert.AreEqual(0, result.TotalDeductions);
            Assert.AreEqual(1200, result.NetSalary);
        }

        [Test]
        public void GetDashboardData_DeductionsButNoSalary_ReturnsZeroValues()
        {
            int id = 50;
            DateTime date = DateTime.Parse("2025-03-01");

            _service.AddPayrollDeduction(new EmployeeDeductionsByPayrollModel
            {
                EmployeeId = id,
                DeductionAmount = 200,
                CreatedDate = date
            });

            var result = _service.GetDashboardData(id, date);

            Assert.NotNull(result);
            Assert.AreEqual(0, result!.CrudSalary);
            Assert.AreEqual(200, result.TotalDeductions);
            Assert.AreEqual(-200, result.NetSalary);
            Assert.AreEqual(0, result.ReteinedPercentage); 
        }
    }
}
