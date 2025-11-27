using NUnit.Framework;
using Moq;
using backend.Services;
using backend.Interfaces;
using backend.Models.Payroll;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace backend_test
{
    public class EmployeeHistoricalReportServiceTests
    {
        private Mock<IEmployeeHistoricalReportRepository> _mockRepo;
        private Mock<ILogger<EmployeeHistoricalReportService>> _mockLogger;
        private EmployeeHistoricalReportService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IEmployeeHistoricalReportRepository>();
            _mockLogger = new Mock<ILogger<EmployeeHistoricalReportService>>();
            _service = new EmployeeHistoricalReportService(_mockRepo.Object, _mockLogger.Object);
        }

        [Test]
        public async Task GenerateReportAsync_ReturnsEmpty_WhenRepositoryReturnsEmpty()
        {
            _mockRepo.Setup(r => r.GetReportAsync(1, null, null))
                     .ReturnsAsync(new List<EmployeeHistoricalReportDto>());

            var result = await _service.GenerateReportAsync(1, null, null);

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GenerateReportAsync_ReturnsReportWithTotalsRow()
        {
            var repoData = new List<EmployeeHistoricalReportDto>
            {
                new EmployeeHistoricalReportDto
                {
                    Salary = 1000,
                    DeductionsAmount = 200,
                    BenefitsAmount = 50,
                    NetSalary = 850
                },
                new EmployeeHistoricalReportDto
                {
                    Salary = 2000,
                    DeductionsAmount = 300,
                    BenefitsAmount = 150,
                    NetSalary = 1850
                }
            };

            _mockRepo.Setup(r => r.GetReportAsync(5, null, null))
                     .ReturnsAsync(repoData);

            var result = (await _service.GenerateReportAsync(5, null, null)).ToList();

            Assert.AreEqual(3, result.Count); // 2 registros + 1 total row

            var totals = result.Last();

            Assert.AreEqual(3000, totals.Salary);
            Assert.AreEqual(500, totals.DeductionsAmount);
            Assert.AreEqual(200, totals.BenefitsAmount);
            Assert.AreEqual(2700, totals.NetSalary);

            Assert.IsNull(totals.PaymentDate);
            Assert.IsEmpty(totals.Position);
            Assert.IsEmpty(totals.EmploymentType);
        }

        [Test]
        public async Task GenerateReportAsync_CallsRepositoryWithCorrectParameters()
        {
            _mockRepo.Setup(r => r.GetReportAsync(10, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                     .ReturnsAsync(new List<EmployeeHistoricalReportDto>());

            DateTime start = new DateTime(2024, 1, 1);
            DateTime end = new DateTime(2024, 12, 31);

            await _service.GenerateReportAsync(10, start, end);

            _mockRepo.Verify(r => r.GetReportAsync(10, start, end), Times.Once);
        }

        [Test]
        public void GenerateReportAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            _mockRepo.Setup(r => r.GetReportAsync(1, null, null))
                     .ThrowsAsync(new Exception("DB error"));

            Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.GenerateReportAsync(1, null, null);
            });
        }

        [Test]
        public async Task GenerateReportAsync_ComputesTotalsCorrectly_WithNegativeValues()
        {
            var repoData = new List<EmployeeHistoricalReportDto>
            {
                new EmployeeHistoricalReportDto
                {
                    Salary = -100,
                    DeductionsAmount = -50,
                    BenefitsAmount = 20,
                    NetSalary = -130
                },
                new EmployeeHistoricalReportDto
                {
                    Salary = 400,
                    DeductionsAmount = 100,
                    BenefitsAmount = -10,
                    NetSalary = 290
                }
            };

            _mockRepo.Setup(r => r.GetReportAsync(99, null, null))
                     .ReturnsAsync(repoData);

            var result = (await _service.GenerateReportAsync(99, null, null)).ToList();

            var totals = result.Last();

            Assert.AreEqual(300, totals.Salary);
            Assert.AreEqual(50, totals.DeductionsAmount);
            Assert.AreEqual(10, totals.BenefitsAmount);
            Assert.AreEqual(160, totals.NetSalary);
        }
    }
}
