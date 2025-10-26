USE Fiesta_Fries_DB;
GO

-- Tabla para almacenar las escalas del impuesto sobre la renta
CREATE TABLE PersonalIncomeTax(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    [Name] VARCHAR(100) NOT NULL, -- Ej: "Escala 1", "Escala 2", etc.
    MinAmount DECIMAL(18,2) NOT NULL, -- Monto mínimo del rango
    MaxAmount DECIMAL(18,2) NULL, -- Monto máximo del rango (NULL para el último rango)
    [Percentage] DECIMAL(5,4) NOT NULL, -- Ej: 0.10 para 10%
    BaseAmount DECIMAL(18,2) NOT NULL DEFAULT 0, -- Monto base a descontar
    [Active] BIT NOT NULL DEFAULT 1, -- Para desactivar escalas sin eliminarlas
    CreationDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModificationDate DATETIME NULL
);
GO

-- Insertar las escalas del impuesto sobre la renta de Costa Rica (2024) - VALORES CORREGIDOS
INSERT INTO PersonalIncomeTax ([Name], MinAmount, MaxAmount, [Percentage], BaseAmount, [Active])
VALUES 
    ('Escala 1', 0.00, 922000.00, 0.00, 0.00, 1),              -- 0% hasta 922,000
    ('Escala 2', 922001.00, 1352000.00, 0.10, 0.00, 1),        -- 10% de 922,001 a 1,352,000
    ('Escala 3', 1352001.00, 2373000.00, 0.15, 43000.00, 1),   -- 15% de 1,352,001 a 2,373,000
    ('Escala 4', 2373001.00, 4745000.00, 0.20, 196150.00, 1),  -- 20% de 2,373,001 a 4,745,000
    ('Escala 5', 4745001.00, NULL, 0.25, 670550.00, 1);        -- 25% más de 4,745,000
GO