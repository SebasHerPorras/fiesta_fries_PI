use Fiesta_Fries_DB
GO

CREATE OR ALTER PROCEDURE SP_Benefit_PhysicalDeletion
    @IdBeneficio INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Eliminar relaciones primero
        DELETE FROM [Fiesta_Fries_DB].[EmployeeBenefit]
        WHERE benefitId = @IdBeneficio;

        -- Eliminar el beneficio
        DELETE FROM [Fiesta_Fries_DB].[Beneficio]
        WHERE IdBeneficio = @IdBeneficio;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE SP_Benefit_LogicalDeletion
    @IdBeneficio INT,
    @LastPeriod DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Marcar beneficio como eliminado
        UPDATE [Fiesta_Fries_DB].[Beneficio]
        SET IsDeleted = 1,
            LastPeriod = @LastPeriod
        WHERE IdBeneficio = @IdBeneficio;

        -- Marcar relaciones EmployeeBenefit como eliminadas
        UPDATE [Fiesta_Fries_DB].[EmployeeBenefit]
        SET IsDeleted = 1
        WHERE benefitId = @IdBeneficio;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO
