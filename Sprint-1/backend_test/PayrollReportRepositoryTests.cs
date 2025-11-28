using backend.Models.Payroll.Results;
using backend.Repositories;
using Dapper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

public class PayrollReportRepositoryTests
{

    [Fact]
    public async Task GetLast12PaymentsByEmployeeAsync_IntegrationTest()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PayrollReportRepository>>();
        var repo = new PayrollReportRepository(loggerMock.Object);

        // Usa un employeeId válido de tu base de datos de pruebas
        int employeeId = 1;

        // Act
        var result = await repo.GetLast12PaymentsByEmployeeAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count <= 12);
        // Puedes agregar más asserts según tus datos de prueba
    }
}