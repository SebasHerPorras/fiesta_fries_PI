uSE [G02-2025-II-DB];
GO

CREATE OR ALTER FUNCTION [Fiesta_Fries_DB].CanEmployeeSelectBenefit(
    @EmployeeId INT,
    @BenefitId INT
)
RETURNS BIT
AS
BEGIN
    DECLARE @CanSelect BIT = 1;

    IF NOT EXISTS (
        SELECT 1 
        FROM Empleado 
        WHERE id = @EmployeeId
    )
    BEGIN
        SET @CanSelect = 0;
        RETURN @CanSelect;
    END

    IF NOT EXISTS (
        SELECT 1 
        FROM Beneficio 
        WHERE idBeneficio = @BenefitId
          AND IsDeleted = 0
    )
    BEGIN
        SET @CanSelect = 0;
        RETURN @CanSelect;
    END

    -- Check if already selected (and is Active)
    IF EXISTS (
        SELECT 1 
        FROM EmployeeBenefit 
        WHERE employeeId = @EmployeeId 
          AND benefitId = @BenefitId
          AND IsDeleted = 0
    )
    BEGIN
        SET @CanSelect = 0;
        RETURN @CanSelect;
    END

    -- Check benefit limit
    DECLARE @CurrentBenefitsCount INT;
    DECLARE @MaxBenefits INT;

    SELECT 
        @CurrentBenefitsCount = COUNT(*),
        @MaxBenefits = e.NoMaxBeneficios
    FROM EmployeeBenefit eb
    INNER JOIN Empleado emp ON eb.employeeId = emp.id
    INNER JOIN Empresa e ON emp.idCompny = e.CedulaJuridica
    WHERE eb.employeeId = @EmployeeId
      AND eb.IsDeleted = 0
    GROUP BY e.NoMaxBeneficios;

    -- Manejo de error
    IF @CurrentBenefitsCount IS NULL
        SET @CurrentBenefitsCount = 0;

    IF @CurrentBenefitsCount >= @MaxBenefits
    BEGIN
        SET @CanSelect = 0;
        RETURN @CanSelect;
    END

    RETURN @CanSelect;
END
GO
