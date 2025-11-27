using Xunit;
using System;
using System.Linq;

public class EmployerDashboard_Tests
{
    [Fact]
    public void GetUltimasFechasPago_ShouldReturnTop3Dates()
    {
        // Arrange
        var repo = new EmpresaRepositoryTests();

        repo.FechasPago = new List<DateTime>
        {
            new DateTime(2025, 11, 25),
            new DateTime(2025, 11, 19),
            new DateTime(2025, 11, 18),
            new DateTime(2025, 11, 10)
        };

        var service = new EmployerDashboardServiceTest(repo);

        // Act
        var result = service.GetUltimasFechasPago(3101123456, new DateTime(2025, 11, 30));

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(new DateTime(2025, 11, 25), result[0]);
        Assert.Equal(new DateTime(2025, 11, 19), result[1]);
        Assert.Equal(new DateTime(2025, 11, 18), result[2]);
    }
    [Fact]
    public void GetPlanillaCosto_ShouldComputeTotalCorrectly()
    {
        // Arrange
        var repo = new EmpresaRepositoryTests();
        repo.Deducciones = 500000;
        repo.Beneficios = 200000;
        repo.Salarios = 1000000;

        var service = new EmployerDashboardServiceTest(repo);

        // Act
        decimal result = service.GetPlanillaCosto(3101123456, new DateTime(2025, 11, 25));

        // Assert
        Assert.Equal(1700000, result);
    }

}
