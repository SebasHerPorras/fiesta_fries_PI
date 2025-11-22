USE Fiesta_Fries_DB;
GO

-- =============================================
-- SCRUM-99: Agregar campos para borrado lógico
-- =============================================

-- 1. Agregar IsDeleted a tabla Empleado
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Empleado') 
    AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE Empleado
    ADD IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedDate DATETIME NULL;
    
    PRINT 'Campos IsDeleted agregados a tabla Empleado';
END
ELSE
BEGIN
    PRINT 'Campos IsDeleted ya existen en tabla Empleado';
END
GO

-- 2. Agregar IsDeleted a tabla Persona
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Persona') 
    AND name = 'IsDeleted'
)
BEGIN
    ALTER TABLE Persona
    ADD IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedDate DATETIME NULL;
    
    PRINT 'Campos IsDeleted agregados a tabla Persona';
END
ELSE
BEGIN
    PRINT 'Campos IsDeleted ya existen en tabla Persona';
END
GO

-- 3. Crear índices para mejorar rendimiento en consultas de empleados activos
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Empleado_IsDeleted_Company' 
    AND object_id = OBJECT_ID('Empleado')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Empleado_IsDeleted_Company
    ON Empleado(IsDeleted, idCompny)
    INCLUDE (id, position, department);
    
    PRINT 'Índice IX_Empleado_IsDeleted_Company creado';
END
ELSE
BEGIN
    PRINT 'Índice IX_Empleado_IsDeleted_Company ya existe';
END
GO

-- 4. Índice para Persona activa
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Persona_IsDeleted_Type' 
    AND object_id = OBJECT_ID('Persona')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Persona_IsDeleted_Type
    ON Persona(IsDeleted, personType)
    INCLUDE (id, uniqueUser);
    
    PRINT 'Índice IX_Persona_IsDeleted_Type creado';
END
ELSE
BEGIN
    PRINT 'Índice IX_Persona_IsDeleted_Type ya existe';
END
GO

-- 5. Actualizar vista/consultas existentes para excluir eliminados
PRINT 'ALTER TABLE completado. Campos IsDeleted disponibles.';
GO