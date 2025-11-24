use Fiesta_Fries_DB
go

CREATE NONCLUSTERED INDEX IX_Payroll_LastModified
ON Payroll (LastModified);
go


CREATE OR ALTER PROCEDURE GetEmployerHistoricalReport
    @EmployerId INT,
    @CompanyId INT = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SELECT 
        e.Nombre,
        e.FrecuenciaPago,
        p.PeriodDate AS PeriodoPago,
        p.LastModified AS FechaPago,
        p.TotalGrossSalary AS SalarioBruto,
        ISNULL(ss.TotalCargasSociales, 0) AS CargasSocialesEmpleador,
        ISNULL(bd.TotalDeducciones, 0) AS DeduccionesVoluntarias,
        p.TotalEmployerCost AS CostoEmpleador
    FROM Payroll p
    INNER JOIN Empresa e ON p.CompanyId = e.CedulaJuridica
    LEFT JOIN (
        SELECT ReportId, SUM(Amount) AS TotalCargasSociales
        FROM EmployerSocialSecurityByPayroll
        GROUP BY ReportId
    ) ss ON p.PayrollId = ss.ReportId
    LEFT JOIN (
        SELECT ReportId, SUM(DeductionAmount) AS TotalDeducciones
        FROM EmployerBenefitDeductions
        GROUP BY ReportId
    ) bd ON p.PayrollId = bd.ReportId
    WHERE (@CompanyId IS NOT NULL AND p.CompanyId = @CompanyId) -- Si viene id compañia, solo esa
      OR (@CompanyId IS NULL AND e.DueñoEmpresa = @EmployerId) -- Sino, todas las del empleador
      AND (@StartDate IS NULL OR p.LastModified >= @StartDate)
      AND (@EndDate IS NULL OR p.LastModified < DATEADD(DAY, 1, @EndDate))
    ORDER BY p.LastModified DESC;
END

go
