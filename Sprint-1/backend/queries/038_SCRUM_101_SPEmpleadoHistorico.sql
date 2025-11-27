use Fiesta_Fries_DB;

CREATE OR ALTER PROCEDURE sp_GetPayrollByEmployeeAndDates
(
    @Cedula INT,
    @FechaInicio DATE = NULL,
    @FechaFin DATE = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
        BEGIN TRANSACTION;

        SELECT  
            e.employmentType,
            e.position,
            CAST(p.PaymentDate AS DATE) AS PaymentDate,
            e.salary,
            p.DeductionsAmount,
            p.BenefitsAmount,
            p.NetSalary
        FROM Empleado e
        INNER JOIN PayrollPayment p 
            ON e.id = p.EmployeeId
        WHERE 
            e.id = @Cedula
            AND (
                    @FechaInicio IS NULL 
                    OR p.PaymentDate >= @FechaInicio
                )
            AND (
                    @FechaFin IS NULL
                    OR p.PaymentDate < DATEADD(DAY, 1, @FechaFin)
                );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END;
GO
