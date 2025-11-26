USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

CREATE TABLE [Fiesta_Fries_DB].[EmployeeBenefit](
    employeeId INT NOT NULL,
    benefitId INT NOT NULL,
    pensionType char(1) NULL, -- API: Pension, Tipo 'A', 'B' o 'C'
    dependentsCount int NULL  -- API: Seguro Priv

    PRIMARY KEY (employeeId, benefitId),
    FOREIGN KEY (employeeId) REFERENCES Empleado(id),
    FOREIGN KEY (benefitId) REFERENCES Beneficio(idBeneficio),
);


ALTER TABLE [Fiesta_Fries_DB].[EmployeeBenefit]
ADD apiName NVARCHAR(100) NULL,
    benefitValue DECIMAL(10,2) NULL,
    benefitType NVARCHAR(20) NULL;
