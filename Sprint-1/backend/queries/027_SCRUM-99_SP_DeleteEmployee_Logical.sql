USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

-- =============================================
-- SCRUM-99: Stored Procedure para borrado LÓGICO
-- =============================================
-- Marca empleado como eliminado (IsDeleted = 1)
-- en Empleado, Persona y desactiva User
-- =============================================

CREATE OR ALTER PROCEDURE [Fiesta_Fries_DB].SP_DeleteEmployee_Logical
    @PersonaId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Variables para control
    DECLARE @UserId UNIQUEIDENTIFIER;
    DECLARE @RowsAffected INT = 0;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- 1. Validar que el empleado existe
        IF NOT EXISTS (SELECT 1 FROM Empleado WHERE id = @PersonaId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'Empleado no encontrado' AS Message;
            RETURN;
        END
        
        -- 2. Validar que no esté ya eliminado
        IF EXISTS (SELECT 1 FROM Empleado WHERE id = @PersonaId AND IsDeleted = 1)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'El empleado ya está marcado como eliminado' AS Message;
            RETURN;
        END
        
        -- 3. Obtener UserId asociado
        SELECT @UserId = uniqueUser 
        FROM Persona 
        WHERE id = @PersonaId;
        
        -- 4. Marcar Empleado como eliminado
        UPDATE [Fiesta_Fries_DB].[Empleado]
        SET IsDeleted = 1,
            DeletedDate = GETDATE()
        WHERE id = @PersonaId;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        IF @RowsAffected = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'No se pudo actualizar el registro de Empleado' AS Message;
            RETURN;
        END
        
        -- 5. Marcar Persona como eliminada
        UPDATE [Fiesta_Fries_DB].[Persona]
        SET IsDeleted = 1,
            DeletedDate = GETDATE()
        WHERE id = @PersonaId;
        
        -- 6. Desactivar User (solo si existe y no es dueño de empresa)
        IF @UserId IS NOT NULL 
           AND NOT EXISTS (SELECT 1 FROM Empresa WHERE DueñoEmpresa = @PersonaId)
        BEGIN
            UPDATE [Fiesta_Fries_DB].[User]
            SET active = 0
            WHERE PK_User = @UserId;
        END
        
        -- 7. Confirmar transacción
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 
               'Empleado marcado como eliminado correctamente (borrado lógico)' AS Message;
               
    END TRY
    BEGIN CATCH
        -- Revertir cambios en caso de error
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Retornar error
        SELECT 0 AS Success, 
               'Error en borrado lógico: ' + ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO