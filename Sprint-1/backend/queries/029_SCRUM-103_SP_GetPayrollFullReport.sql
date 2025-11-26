USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

-- =============================================
-- SCRUM-103: Stored Procedure para reporte completo de planilla
-- =============================================
-- Retorna todos los datos necesarios para generar reportes PDF/CSV
-- =============================================

CREATE OR ALTER PROCEDURE [Fiesta_Fries_DB].SP_GetPayrollFullReport
    @PayrollId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. ENCABEZADO DEL REPORTE
    SELECT 
        p.PayrollId,
        p.PeriodDate,
        p.CompanyId,
        e.Nombre AS NombreEmpresa,
        per.firstName + ' ' + per.secondName AS NombreEmpleador,
        e.DiaPago,
        e.FrecuenciaPago,
        p.TotalGrossSalary,
        p.TotalEmployeeDeductions,
        p.TotalEmployerDeductions,
        p.TotalBenefits,
        p.TotalNetSalary,
        p.TotalEmployerCost,
        p.ApprovedBy,
        p.LastModified
    FROM Payroll p
    INNER JOIN Empresa e ON p.CompanyId = e.CedulaJuridica
    INNER JOIN Persona per ON e.DueñoEmpresa = per.id
    WHERE p.PayrollId = @PayrollId;

    -- 2. DETALLE POR EMPLEADO
    SELECT 
        pp.EmployeeId,
        pers.firstName + ' ' + pers.secondName AS NombreEmpleado,
        emp.employmentType AS TipoEmpleado,
        pp.GrossSalary,
        pp.DeductionsAmount,
        pp.BenefitsAmount,
        pp.NetSalary,
        pp.Status
    FROM PayrollPayment pp
    INNER JOIN Persona pers ON pp.EmployeeId = pers.id
    INNER JOIN Empleado emp ON pp.EmployeeId = emp.id
    WHERE pp.PayrollId = @PayrollId
    ORDER BY pers.firstName, pers.secondName;

    -- 3. DEDUCCIONES EMPLEADOR (Cargas Sociales)
    SELECT 
        ChargeName,
        SUM(Amount) AS TotalAmount,
        AVG(Percentage) * 100 AS PercentageDisplay
    FROM EmployerSocialSecurityByPayroll
    WHERE ReportId = @PayrollId
    GROUP BY ChargeName
    ORDER BY ChargeName;

    -- 4. DEDUCCIONES EMPLEADO (CCSS)
    SELECT 
        DeductionName,
        SUM(DeductionAmount) AS TotalAmount,
        AVG(Percentage) * 100 AS PercentageDisplay
    FROM EmployeeDeductionsByPayroll
    WHERE ReportId = @PayrollId
    GROUP BY DeductionName
    ORDER BY DeductionName;

    -- 5. BENEFICIOS EMPRESARIALES
    SELECT 
        BenefitName,
        BenefitType,
        SUM(DeductionAmount) AS TotalAmount
    FROM EmployerBenefitDeductions
    WHERE ReportId = @PayrollId
    GROUP BY BenefitName, BenefitType
    ORDER BY BenefitName;
END;
GO


select * from PayrollPayment
select * from Empresa