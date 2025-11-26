use Fiesta_Fries_DB
GO

ALTER TABLE [Fiesta_Fries_DB].[Beneficio]
ADD IsDeleted BIT NOT NULL DEFAULT(0),
    LastPeriod datetime NULL;

ALTER TABLE [Fiesta_Fries_DB].[EmployeeBenefit]
ADD IsDeleted BIT NOT NULL DEFAULT(0);


ALTER TABLE [Fiesta_Fries_DB].[EmployerBenefitDeductions]
ADD BenefitId INT NULL;

-- FK a Beneficio
ALTER TABLE [Fiesta_Fries_DB].[EmployerBenefitDeductions]
ADD CONSTRAINT FK_EmployerBenefitDeductions_Beneficio
    FOREIGN KEY (BenefitId) REFERENCES Beneficio(IdBeneficio);
GO


CREATE OR ALTER PROCEDURE SP_InsertEmployerBenefitDeduction
    @ReportId INT,
    @EmployeeId INT,
    @CedulaJuridicaEmpresa BIGINT,
    @BenefitName VARCHAR(100),
    @BenefitId INT = NULL,
    @DeductionAmount DECIMAL(18,2),
    @BenefitType VARCHAR(50),
    @Percentage DECIMAL(6,4) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO [Fiesta_Fries_DB].[EmployerBenefitDeductions] (
            ReportId,
            EmployeeId,
            CedulaJuridicaEmpresa,
            BenefitName,
            BenefitId,
            DeductionAmount,
            BenefitType,
            Percentage,
            CreatedDate
        )
        VALUES (
            @ReportId,
            @EmployeeId,
            @CedulaJuridicaEmpresa,
            @BenefitName,
            @BenefitId,
            @DeductionAmount,
            @BenefitType,
            @Percentage,
            GETDATE()
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
