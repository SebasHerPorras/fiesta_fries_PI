USE Fiesta_Fries_DB;
GO

-- Tabla para almacenar las tasas patronales (deducciones de empleador)
CREATE TABLE EmployerSocialSecurityContributions(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Name] VARCHAR(100) NOT NULL, -- Ej: "CCSS Salud", "INA", etc.
    [Percentage] DECIMAL(5,4) NOT NULL, -- Ej: 0.0925 para 9.25%
    [Active] BIT NOT NULL DEFAULT 1, -- Para desactivar tasas sin eliminarlas
    CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModificationDate DATETIME NULL
);
GO

-- Insertar las tasas patronales actuales de Costa Rica
INSERT INTO EmployerSocialSecurityContributions ([Name],  [Percentage], [Active])
VALUES 
    ('CCSS Salud', 0.0925, 1),                           -- 9.25%
    ('CCSS Pensiones (IVM)', 0.0525, 1),                 -- 5.25%
    ('INA', 0.005, 1),                                   -- 0.50%
    ('ASFA', 0.0025, 1),                                 -- 0.25%
    ('Banco Popular', 0.0025, 1),                        -- 0.25%
    ('Fondo de Capitalización Laboral (FCL)', 0.03, 1); -- 3.00%
GO