-- =============================================
-- Script: Crear Schema Fiesta_Fries_DB en Azure SQL Database
-- Database: G02-2025-II-DB
-- Descripción: Crea el schema Fiesta_Fries_DB y configura el usuario grupo2 con default schema
-- Fecha: 26 de noviembre de 2025
-- =============================================

USE [G02-2025-II-DB];
GO

-- =============================================
-- 1. Crear el schema Fiesta_Fries_DB
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Fiesta_Fries_DB')
BEGIN
    EXEC('CREATE SCHEMA [Fiesta_Fries_DB] AUTHORIZATION dbo');
    PRINT '✓ Schema [Fiesta_Fries_DB] creado exitosamente';
END
ELSE
BEGIN
    PRINT '⚠ El schema [Fiesta_Fries_DB] ya existe';
END
GO

-- =============================================
-- 2. Configurar el usuario grupo2 con default schema
-- =============================================
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'grupo2' AND type = 'S')
BEGIN
    ALTER USER [grupo2] WITH DEFAULT_SCHEMA = [Fiesta_Fries_DB];
    PRINT '✓ Usuario [grupo2] configurado con default schema [Fiesta_Fries_DB]';
END
ELSE
BEGIN
    PRINT '⚠ Usuario [grupo2] no encontrado';
END
GO

-- =============================================
-- 3. Verificar la configuración
-- =============================================
PRINT '';
PRINT '=== VERIFICACIÓN DE CONFIGURACIÓN ===';
PRINT '';

-- Verificar schema
SELECT 
    'Schema creado' AS Verificacion,
    name AS NombreSchema,
    schema_id AS SchemaID,
    USER_NAME(principal_id) AS Propietario
FROM sys.schemas 
WHERE name = 'Fiesta_Fries_DB';

-- Verificar usuario
SELECT 
    'Usuario configurado' AS Verificacion,
    name AS NombreUsuario,
    default_schema_name AS DefaultSchema,
    type_desc AS TipoUsuario,
    create_date AS FechaCreacion
FROM sys.database_principals
WHERE name = 'grupo2';

PRINT '';
PRINT '✓ Configuración completada exitosamente';
PRINT '';
PRINT 'NOTA: Ahora todas las consultas SQL sin prefijo de schema usarán automáticamente [Fiesta_Fries_DB]';
PRINT 'Ejemplo: SELECT * FROM Empleado --> SELECT * FROM Fiesta_Fries_DB.Empleado';
GO
