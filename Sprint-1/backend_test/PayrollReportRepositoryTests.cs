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
    public async Task GetLast12PaymentsByEmployeeAsync_ReturnsExpectedResults()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<PayrollReportRepository>>();

        // Mock la conexión y Dapper
        var connectionMock = new Mock<IDbConnection>();
        var expectedList = new List<EmployeeLastPaymentsResult>
        {
            new EmployeeLastPaymentsResult
            {
                ReportId = 1,
                Periodo = "2024-01-01",
                SalarioBruto = 1000,
                SalarioNeto = 900
            }
        };

        // Mock Dapper QueryAsync
        SqlMapper.AddTypeMap(typeof(EmployeeLastPaymentsResult), System.Data.DbType.Object);
        var dapperMock = new Mock<IDbConnection>();
        dapperMock.SetupDapperAsync(c =>
            c.QueryAsync<EmployeeLastPaymentsResult>(
                It.IsAny<string>(),
                It.IsAny<object>(),
                null, null, null))
            .ReturnsAsync(expectedList);

        // Crea el repositorio usando el logger mockeado
        var repo = new PayrollReportRepository(loggerMock.Object);

        // Act
        var result = await repo.GetLast12PaymentsByEmployeeAsync(123);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].ReportId);
        Assert.Equal("2024-01-01", result[0].Periodo);
        Assert.Equal(1000, result[0].SalarioBruto);
        Assert.Equal(900, result[0].SalarioNeto);
    }

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