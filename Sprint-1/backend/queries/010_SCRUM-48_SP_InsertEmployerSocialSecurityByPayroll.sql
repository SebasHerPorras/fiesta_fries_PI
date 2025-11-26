USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

CREATE OR ALTER PROCEDURE SP_InsertEmployerSocialSecurityByPayroll
    @ReportId INT,
    @EmployeeId BIGINT,
    @ChargeName VARCHAR(100),
    @Amount DECIMAL(18,2),
    @Percentage DECIMAL(5,4),
    @CedulaJuridicaEmpresa BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO [Fiesta_Fries_DB].[EmployerSocialSecurityByPayroll] (
            ReportId, 
            EmployeeId, 
            ChargeName, 
            Amount, 
            Percentage,
            CedulaJuridicaEmpresa
        )
        VALUES (
            @ReportId, 
            @EmployeeId, 
            @ChargeName, 
            @Amount, 
            @Percentage,
            @CedulaJuridicaEmpresa
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

select * from EmployerSocialSecurityByPayroll