USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

-- Tabla para almacenar las deducciones de beneficios del empleador por planilla
CREATE TABLE [Fiesta_Fries_DB].[EmployerBenefitDeductions](
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReportId INT NOT NULL,
    EmployeeId INT NOT NULL,
    CedulaJuridicaEmpresa BIGINT NOT NULL,
    BenefitName VARCHAR(100) NOT NULL,
    DeductionAmount DECIMAL(18,2) NOT NULL,
    BenefitType VARCHAR(50) NOT NULL, -- "Fixed", "Percentage", "API"
    Percentage DECIMAL(5,4) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT FK_EmployerBenefitDeductions_Employee FOREIGN KEY (EmployeeId) REFERENCES Empleado(id),
    CONSTRAINT FK_EmployerBenefitDeductions_Company FOREIGN KEY (CedulaJuridicaEmpresa) REFERENCES Empresa(CedulaJuridica)
);
GO

-- Alterar la tabla para cambiar el tipo de dato de Percentage
ALTER TABLE [Fiesta_Fries_DB].[EmployerBenefitDeductions]
ALTER COLUMN Percentage DECIMAL(6,4) NULL;
GO

-- Crear �ndices para mejorar el rendimiento de consultas
CREATE INDEX IX_EmployerBenefitDeductions_ReportId ON [Fiesta_Fries_DB].[EmployerBenefitDeductions](ReportId);
CREATE INDEX IX_EmployerBenefitDeductions_EmployeeId ON [Fiesta_Fries_DB].[EmployerBenefitDeductions](EmployeeId);
CREATE INDEX IX_EmployerBenefitDeductions_Company ON [Fiesta_Fries_DB].[EmployerBenefitDeductions](CedulaJuridicaEmpresa);
GO

-- Stored Procedure para insertar deducciones de beneficios
CREATE OR ALTER PROCEDURE [Fiesta_Fries_DB].SP_InsertEmployerBenefitDeduction
    @ReportId INT,
    @EmployeeId INT,
    @CedulaJuridicaEmpresa BIGINT,
    @BenefitName VARCHAR(100),
    @DeductionAmount DECIMAL(18,2),
    @BenefitType VARCHAR(50),
    @Percentage DECIMAL(6,4) = NULL  -- CAMBIO: de DECIMAL(5,4) a DECIMAL(6,4)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        INSERT INTO [Fiesta_Fries_DB].[EmployerBenefitDeductions] (
            ReportId,
            EmployeeId,
            CedulaJuridicaEmpresa,
            BenefitName,
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

