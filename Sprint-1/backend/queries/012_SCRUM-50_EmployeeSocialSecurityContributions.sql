USE Fiesta_Fries_DB;
GO

-- Tabla para almacenar las deducciones del empleado (CCSS)
CREATE TABLE EmployeeSocialSecurityContributions(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Name] VARCHAR(100) NOT NULL, -- Ej: "CCSS Salud Empleado", "CCSS Pensiones Empleado"
    [Percentage] DECIMAL(5,4) NOT NULL, -- Ej: 0.055 para 5.5%, 0.04 para 4%
    [Active] BIT NOT NULL DEFAULT 1,
    CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModificationDate DATETIME NULL
);
GO

-- Insertar las deducciones de empleado de CCSS
INSERT INTO EmployeeSocialSecurityContributions ([Name], [Percentage], [Active])
VALUES 
    ('CCSS Salud Empleado', 0.055, 1),           -- 5.5%
    ('CCSS Pensiones Empleado (IVM)', 0.04, 1);  -- 4.0%
GO