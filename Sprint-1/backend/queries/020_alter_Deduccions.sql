---- Insertar las tasas patronales actuales de Costa Rica
--INSERT INTO [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] ([Name],  [Percentage], [Active])
--VALUES 
--    ('CCSS Salud', 0.0925, 1),                           -- 9.25%
--    ('CCSS Pensiones (IVM)', 0.0525, 1),                 -- 5.25%
--    ('INA', 0.005, 1),                                   -- 0.50%
--    ('ASFA', 0.0025, 1),                                 -- 0.25%
--    ('Banco Popular', 0.0025, 1),                        -- 0.25%
--    ('Fondo de Capitalizaci�n Laboral (FCL)', 0.03, 1); -- 3.00%
--    -- total patronal aproximado: 20.75%
--GO

-- Insertar las deducciones de empleado de CCSS
--INSERT INTO [Fiesta_Fries_DB].[EmployeeSocialSecurityContributions] ([Name], [Percentage], [Active])
--VALUES 
--    ('CCSS Salud Empleado', 0.055, 1),           -- 5.5%
--    ('CCSS Pensiones Empleado (IVM)', 0.04, 1);  -- 4.0%
--GO

USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

-- ========================================
-- ACTUALIZAR CARGAS SOCIALES PATRONALES EXISTENTES
-- ========================================

-- CCSS Salud (SEM) - Actualizar a 9.25%
UPDATE [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] SET [Percentage] = 0.0925
WHERE [Name] = 'CCSS Salud';

-- CCSS Pensiones (IVM) - Actualizar de 5.25% a 5.42%
UPDATE [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] SET [Percentage] = 0.0542
WHERE [Name] = 'CCSS Pensiones (IVM)';

-- INA - Actualizar de 0.50% a 1.50%
UPDATE [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] SET [Percentage] = 0.010
WHERE [Name] = 'INA';

-- ASFA - Mantener en 0.25%
UPDATE [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] SET [Percentage] = 0.0025
WHERE [Name] = 'ASFA';

-- Banco Popular - Mantener en 0.25%
UPDATE [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] SET [Percentage] = 0.0025
WHERE [Name] = 'Banco Popular';

-- FCL - Actualizar de 3.00% a 1.50%
UPDATE [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] SET [Percentage] = 0.0150
WHERE [Name] = 'Fondo de Capitalizaci�n Laboral (FCL)';

PRINT 'Valores existentes actualizados correctamente';
GO

-- ========================================
-- INSERTAR NUEVAS CARGAS SOCIALES PATRONALES
-- ========================================

-- INS - Riesgos del Trabajo (1.00%)
INSERT INTO [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] ([Name], [Percentage], [Active])
VALUES ('INS - Riesgos del Trabajo', 0.0100, 1);

-- ROP - R�gimen Obligatorio de Pensiones (2.00%)
INSERT INTO [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] ([Name], [Percentage], [Active])
VALUES ('ROP - R�gimen Obligatorio de Pensiones', 0.0200, 1);

-- FODESAF - Fondo de Desarrollo Social (5.00%)
INSERT INTO [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] ([Name], [Percentage], [Active])
VALUES ('FODESAF - Fondo de Desarrollo Social', 0.0500, 1);

-- IMAS - Instituto Mixto de Ayuda Social (0.50%)
INSERT INTO [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] ([Name], [Percentage], [Active])
VALUES ('IMAS - Instituto Mixto de Ayuda Social', 0.0050, 1);

-- Banco Popular - Aporte Institucional (0.50%)
INSERT INTO [Fiesta_Fries_DB].[EmployerSocialSecurityContributions] ([Name], [Percentage], [Active])
VALUES ('Banco Popular - Aporte Institucional', 0.0050, 1);

PRINT 'Nuevas cargas sociales insertadas correctamente';
GO

-- ========================================
-- ACTUALIZAR DEDUCCIONES DE EMPLEADO EXISTENTES
-- ========================================

-- CCSS Salud Empleado - Mantener en 5.5%
UPDATE [Fiesta_Fries_DB].[EmployeeSocialSecurityContributions]
SET [Percentage] = 0.0550
WHERE [Name] = 'CCSS Salud Empleado';

-- CCSS Pensiones Empleado (IVM) - Actualizar de 4.0% a 4.17%
UPDATE [Fiesta_Fries_DB].[EmployeeSocialSecurityContributions]
SET [Percentage] = 0.0417
WHERE [Name] = 'CCSS Pensiones Empleado (IVM)';

PRINT 'Deducciones de empleado actualizadas correctamente';
GO

-- ========================================
-- INSERTAR NUEVA DEDUCCI�N DE EMPLEADO
-- ========================================

-- Banco Popular Empleado - 1.00%
INSERT INTO [Fiesta_Fries_DB].[EmployeeSocialSecurityContributions] ([Name], [Percentage], [Active])
VALUES ('Banco Popular Empleado', 0.0100, 1);

PRINT 'Banco Popular Empleado insertado correctamente';
GO

-- ========================================
-- VERIFICACI�N FINAL
-- ========================================

PRINT '';
PRINT '========================================';
PRINT 'CARGAS SOCIALES PATRONALES';
PRINT '========================================';
SELECT 
    [Name] AS Nombre,
    CONCAT(CAST([Percentage] * 100 AS DECIMAL(5,2)), '%') AS Porcentaje,
    CASE WHEN [Active] = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado
FROM [Fiesta_Fries_DB].EmployerSocialSecurityContributions
ORDER BY [Percentage] DESC;

PRINT '';
SELECT 
    CONCAT('Total Aporte Patronal: ', CAST(SUM([Percentage]) * 100 AS DECIMAL(5,2)), '%') AS Resumen
FROM [Fiesta_Fries_DB].EmployerSocialSecurityContributions
WHERE [Active] = 1;

PRINT '';
PRINT '========================================';
PRINT 'DEDUCCIONES DE EMPLEADO';
PRINT '========================================';
SELECT 
    [Name] AS Nombre,
    CONCAT(CAST([Percentage] * 100 AS DECIMAL(5,2)), '%') AS Porcentaje,
    CASE WHEN [Active] = 1 THEN 'Activo' ELSE 'Inactivo' END AS Estado
FROM [Fiesta_Fries_DB].EmployeeSocialSecurityContributions
ORDER BY [Percentage] DESC;

PRINT '';
SELECT 
    CONCAT('Total Deducciones Empleado: ', CAST(SUM([Percentage]) * 100 AS DECIMAL(5,2)), '%') AS Resumen
FROM [Fiesta_Fries_DB].EmployeeSocialSecurityContributions
WHERE [Active] = 1;
GO