USE Fiesta_Fries_DB;
GO

-- Tabla general para almacenar TODAS las deducciones del empleado por planilla
-- Aquí va tanto impuesto sobre la renta como deducciones CCSS
CREATE TABLE EmployeeDeductionsByPayroll (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReportId INT NOT NULL,
    EmployeeId INT NOT NULL,
    CedulaJuridicaEmpresa BIGINT NOT NULL,
    DeductionName VARCHAR(100) NOT NULL, -- Ej: "Impuesto sobre la Renta", "CCSS Salud Empleado"
    DeductionAmount DECIMAL(18,2) NOT NULL,
    Percentage DECIMAL(5,4) NULL, -- Para deducciones CCSS
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    
    FOREIGN KEY (EmployeeId) REFERENCES Empleado(id),
    FOREIGN KEY (CedulaJuridicaEmpresa) REFERENCES Empresa(CedulaJuridica)
);
GO