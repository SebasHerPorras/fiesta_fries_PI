USE Fiesta_Fries_DB;
GO

-- =============================================
-- SCRUM-103: Stored Procedure para reporte por EMPLEADO en una planilla
-- =============================================
-- Result sets:
-- 1) Header (Empresa + Empleado + Tipo + FechaPago + SalarioBruto)
-- 2) Deducciones obligatorias (EmployeeDeductionsByPayroll)
-- 3) Total deducciones obligatorias (single row)
-- 4) Beneficios / deducciones voluntarias (EmployerBenefitDeductions)
-- 5) Total beneficios voluntarios (single row)
-- 6) Totales finales: TotalDeducciones, TotalBeneficios, TotalDeduccionesTotales, PagoNeto
-- =============================================

CREATE OR ALTER PROCEDURE SP_GetPayrollEmployeeReport
    @PayrollId INT,
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;

    ------------------------------------------------
    -- 1) Encabezado: empresa, empleado, tipo, fecha de pago, salario bruto
    ------------------------------------------------
    SELECT
        e.CedulaJuridica AS CompanyId,
        e.Nombre AS NombreEmpresa,
        p.id AS EmployeeId,
        p.firstName + ' ' + p.secondName AS NombreEmpleado,
        ISNULL(emp.employmentType, '') AS TipoEmpleado,
        pp.PaymentDate AS FechaPago,
        pp.GrossSalary AS SalarioBruto
    FROM PayrollPayment pp
    INNER JOIN Persona p ON pp.EmployeeId = p.id
    LEFT JOIN Empleado emp ON pp.EmployeeId = emp.id
    LEFT JOIN Payroll py ON pp.PayrollId = py.PayrollId
    LEFT JOIN Empresa e ON py.CompanyId = e.CedulaJuridica
    WHERE pp.PayrollId = @PayrollId
      AND pp.EmployeeId = @EmployeeId;

    ------------------------------------------------
    -- 2) Deducciones obligatorias por concepto
    ------------------------------------------------
    SELECT 
        DeductionName,
        DeductionAmount,
        Percentage
    FROM EmployeeDeductionsByPayroll
    WHERE ReportId = @PayrollId
      AND EmployeeId = @EmployeeId
    ORDER BY DeductionName;

    ------------------------------------------------
    -- 3) Total deducciones obligatorias (single row)
    ------------------------------------------------
    SELECT 
        ISNULL(SUM(DeductionAmount), 0) AS TotalDeduccionesObligatorias
    FROM EmployeeDeductionsByPayroll
    WHERE ReportId = @PayrollId
      AND EmployeeId = @EmployeeId;

    ------------------------------------------------
    -- 4) Beneficios / deducciones voluntarias por concepto
    ------------------------------------------------
    SELECT
        BenefitName,
        BenefitType,
        DeductionAmount AS BenefitAmount,
        Percentage
    FROM EmployerBenefitDeductions
    WHERE ReportId = @PayrollId
      AND EmployeeId = @EmployeeId
    ORDER BY BenefitName;

    ------------------------------------------------
    -- 5) Total beneficios voluntarios (single row)
    ------------------------------------------------
    SELECT
        ISNULL(SUM(DeductionAmount), 0) AS TotalBeneficiosVoluntarios
    FROM EmployerBenefitDeductions
    WHERE ReportId = @PayrollId
      AND EmployeeId = @EmployeeId;

    ------------------------------------------------
    -- 6) Totales finales (single row)
    --    TotalDeducciones = obligatorias + voluntarias
    --    PagoNeto = NetSalary from PayrollPayment (fallback calculado si no existe)
    ------------------------------------------------
    SELECT
        ISNULL((SELECT SUM(DeductionAmount) FROM EmployeeDeductionsByPayroll WHERE ReportId = @PayrollId AND EmployeeId = @EmployeeId), 0) AS TotalDeduccionesObligatorias,
        ISNULL((SELECT SUM(DeductionAmount) FROM EmployerBenefitDeductions WHERE ReportId = @PayrollId AND EmployeeId = @EmployeeId), 0) AS TotalBeneficiosVoluntarios,
        ISNULL((SELECT SUM(DeductionAmount) FROM EmployeeDeductionsByPayroll WHERE ReportId = @PayrollId AND EmployeeId = @EmployeeId), 0)
          + ISNULL((SELECT SUM(DeductionAmount) FROM EmployerBenefitDeductions WHERE ReportId = @PayrollId AND EmployeeId = @EmployeeId), 0)
          AS TotalDeducciones,
        ISNULL((SELECT TOP 1 NetSalary FROM PayrollPayment WHERE PayrollId = @PayrollId AND EmployeeId = @EmployeeId), 
               -- fallback: GrossSalary - total deductions (si no hay NetSalary)
               ISNULL((SELECT TOP 1 GrossSalary FROM PayrollPayment WHERE PayrollId = @PayrollId AND EmployeeId = @EmployeeId),0)
               - ISNULL((SELECT SUM(DeductionAmount) FROM EmployeeDeductionsByPayroll WHERE ReportId = @PayrollId AND EmployeeId = @EmployeeId), 0)
               - ISNULL((SELECT SUM(DeductionAmount) FROM EmployerBenefitDeductions WHERE ReportId = @PayrollId AND EmployeeId = @EmployeeId), 0)
              ) AS PagoNeto;

END;
GO

--EXEC SP_GetPayrollEmployeeReport
--   @PayrollId = 1001,
--    @EmployeeId = 309870267;