CREATE OR ALTER PROCEDURE GetEmployerReport
    @CedulaEmpleador INT, 
    @StartDate DATE = NULL,
    @EndDate DATE = NULL,
    @EmploymentType VARCHAR(50) = NULL,
    @CompanyId BIGINT = NULL,
    @Cedula INT = NULL
AS
BEGIN
    SELECT 
        CONCAT(P.firstName, ' ', P.secondName) AS [Nombre],
        P.id AS Cedula,
        E.employmentType AS [Tipo de empleado],
        PY.PeriodDate AS [PeriodoPago],
        PY.LastModified AS [FechaPago],
        E.salary AS [SalarioBruto],
        ISNULL(ESS.TotalCargasSociales, 0) AS [CargasSocialesEmpleador],
        ISNULL(EDD.TotalDeducciones, 0) AS [DeduccionesVoluntarias],
        (E.salary + ISNULL(ESS.TotalCargasSociales, 0)+ ISNULL(EDD.TotalDeducciones, 0)) AS [CostoEmpleador]
    FROM 
        Payroll PY
        INNER JOIN payrollPayment PP ON PY.PayrollId = PP.PayrollId
        INNER JOIN Empleado E ON PP.EmployeeId = E.id
        INNER JOIN Persona P ON E.id = P.id
        INNER JOIN Empresa EMP ON E.idCompny = EMP.cedulaJuridica
        LEFT JOIN (
            SELECT 
                ReportId, 
                EmployeeId,
                SUM(Amount) AS TotalCargasSociales
            FROM EmployerSocialSecurityByPayroll
            GROUP BY ReportId, EmployeeId
        ) ESS ON PY.PayrollId = ESS.ReportId AND E.id = ESS.EmployeeId
        LEFT JOIN (
            SELECT 
                ReportId, 
                EmployeeId,
                SUM(DeductionAmount) AS TotalDeducciones
            FROM EmployeeDeductionsByPayroll
            GROUP BY ReportId, EmployeeId
        ) EDD ON PY.PayrollId = EDD.ReportId AND E.id = EDD.EmployeeId
    WHERE
        (@StartDate IS NULL OR PY.PeriodDate >= @StartDate)
        AND (@EndDate IS NULL OR PY.PeriodDate <= @EndDate)
        AND (@EmploymentType IS NULL OR E.employmentType = @EmploymentType)
        AND (@CompanyId IS NULL OR E.idCompny = @CompanyId)  
        AND (@Cedula IS NULL OR P.id = @Cedula)
        AND (@CedulaEmpleador IS NULL OR EMP.dueñoEmpresa = @CedulaEmpleador)  
    ORDER BY PY.PeriodDate, P.id
END