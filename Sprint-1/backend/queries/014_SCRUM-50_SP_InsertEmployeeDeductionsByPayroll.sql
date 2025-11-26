USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

CREATE OR ALTER PROCEDURE SP_InsertEmployeeDeductionsByPayroll
    @ReportId INT,
    @EmployeeId INT,
    @CedulaJuridicaEmpresa BIGINT,
    @DeductionName VARCHAR(100),
    @DeductionAmount DECIMAL(18,2),
    @Percentage DECIMAL(5,4) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO [Fiesta_Fries_DB].[EmployeeDeductionsByPayroll] (
            ReportId, 
            EmployeeId, 
            CedulaJuridicaEmpresa,
            DeductionName, 
            DeductionAmount,
            Percentage
        )
        VALUES (
            @ReportId, 
            @EmployeeId, 
            @CedulaJuridicaEmpresa,
            @DeductionName, 
            @DeductionAmount,
            @Percentage
        );
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- Query para verificar los datos
SELECT * FROM [Fiesta_Fries_DB].[EmployeeDeductionsByPayroll];