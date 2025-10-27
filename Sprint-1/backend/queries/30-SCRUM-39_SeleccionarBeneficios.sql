USE Fiesta_Fries_DB;
GO

CREATE TABLE EmployeeBenefit (
    employeeId INT NOT NULL,
    benefitId INT NOT NULL,
    pensionType char(1) NULL, -- API: Pension, Tipo 'A', 'B' o 'C'
    dependentsCount int NULL  -- API: Seguro Priv

    PRIMARY KEY (employeeId, benefitId),
    FOREIGN KEY (employeeId) REFERENCES Empleado(id),
    FOREIGN KEY (benefitId) REFERENCES Beneficio(idBeneficio),
);


ALTER TABLE EmployeeBenefit
ADD apiName NVARCHAR(100) NULL,
    benefitValue DECIMAL(10,2) NULL,
    benefitType NVARCHAR(20) NULL;
