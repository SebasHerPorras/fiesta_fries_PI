USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

CREATE PROCEDURE sp_CreatePayrollPayment
    @PayrollId INT,
    @EmployeeId INT,
    @GrossSalary DECIMAL(18,2),
    @DeductionsAmount DECIMAL(18,2),
    @BenefitsAmount DECIMAL(18,2),
    @NetSalary DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [Fiesta_Fries_DB].[PayrollPayment] 
    (PayrollId, EmployeeId, GrossSalary, DeductionsAmount, BenefitsAmount, NetSalary, PaymentDate, Status)
    VALUES 
    (@PayrollId, @EmployeeId, @GrossSalary, @DeductionsAmount, @BenefitsAmount, @NetSalary, GETDATE(), 'PROCESADO')
    
    SELECT 
        PaymentId, 
        PayrollId, 
        EmployeeId, 
        GrossSalary, 
        DeductionsAmount, 
        BenefitsAmount, 
        NetSalary,
        PaymentDate, 
        Status
    FROM PayrollPayment 
    WHERE PaymentId = SCOPE_IDENTITY()
END
GO