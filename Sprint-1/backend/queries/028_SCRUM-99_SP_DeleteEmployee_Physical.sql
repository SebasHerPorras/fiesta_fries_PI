USE Fiesta_Fries_DB;
GO

-- =============================================
-- SCRUM-99: Stored Procedure para borrado FÍSICO
-- =============================================
-- Elimina completamente al empleado y sus datos
-- Solo si NO tiene pagos registrados en planilla
-- =============================================

CREATE OR ALTER PROCEDURE SP_DeleteEmployee_Physical
    @PersonaId INT
AS
BEGIN
    SET NOCOUNT ON;
       
    -- Variables para control
    DECLARE @UserId UNIQUEIDENTIFIER;
    DECLARE @PaymentCount INT = 0;
    
    BEGIN TRANSACTION;
    
    BEGIN TRY
        -- 1. Validar que el empleado existe
        IF NOT EXISTS (SELECT 1 FROM Empleado WHERE id = @PersonaId)
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'Empleado no encontrado' AS Message;
            RETURN;
        END
        
        -- 2. VALIDACIÓN CRÍTICA: Verificar que NO tenga pagos
        SELECT @PaymentCount = COUNT(*)
        FROM PayrollPayment
        WHERE EmployeeId = @PersonaId;
        
        IF @PaymentCount > 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 
                   'ERROR: El empleado tiene ' + CAST(@PaymentCount AS VARCHAR) + 
                   ' pago(s) registrado(s). Debe usar borrado lógico.' AS Message;
            RETURN;
        END
        
        -- 3. Obtener UserId antes de eliminar
        SELECT @UserId = uniqueUser 
        FROM Persona 
        WHERE id = @PersonaId;
        
        -- 4. Eliminar dependencias en orden (integridad referencial)
        
        -- 4.1. Beneficios del empleado
        DELETE FROM EmployeeBenefit 
        WHERE EmployeeId = @PersonaId;
        
        -- 4.2. Horas trabajadas (días)
        DELETE FROM Dia 
        WHERE id_employee = @PersonaId;
        
        -- 4.3. Horas trabajadas (semanas)
        DELETE FROM Semana 
        WHERE id_employee = @PersonaId;
        
        -- 4.4. Deducciones por planilla (si existen, aunque no debería)
        DELETE FROM EmployeeDeductionsByPayroll 
        WHERE EmployeeId = @PersonaId;
        
        -- 4.5. Beneficios patronales por planilla
        DELETE FROM EmployerBenefitDeductions 
        WHERE EmployeeId = @PersonaId;
        
        -- 4.6. Cargas sociales patronales por planilla
        DELETE FROM EmployerSocialSecurityByPayroll 
        WHERE EmployeeId = @PersonaId;
        
        -- 5. Eliminar registro de Empleado
        DELETE FROM Empleado 
        WHERE id = @PersonaId;
        
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'No se pudo eliminar el registro de Empleado' AS Message;
            RETURN;
        END
        
        -- 6. Eliminar registro de Persona
        DELETE FROM Persona 
        WHERE id = @PersonaId;
        
        IF @@ROWCOUNT = 0
        BEGIN
            ROLLBACK TRANSACTION;
            SELECT 0 AS Success, 'No se pudo eliminar el registro de Persona' AS Message;
            RETURN;
        END
        
        -- 7. Eliminar User SOLO si no es dueño de empresa
        IF @UserId IS NOT NULL 
           AND NOT EXISTS (SELECT 1 FROM Empresa WHERE DueñoEmpresa = @PersonaId)
        BEGIN
            -- Eliminar verificaciones de email
            DELETE FROM EmailVerification 
            WHERE userId = @UserId;
            
            DELETE FROM EmailVerificationE 
            WHERE token = CAST(@PersonaId AS NVARCHAR(50));
            
            -- Eliminar usuario
            DELETE FROM [User] 
            WHERE PK_User = @UserId;
        END
        
        -- 8. Confirmar transacción
        COMMIT TRANSACTION;
        
        SELECT 1 AS Success, 
               'Empleado eliminado completamente del sistema (borrado físico)' AS Message;
               
    END TRY
    BEGIN CATCH
        -- Revertir cambios en caso de error
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        
        -- Retornar error detallado
        SELECT 0 AS Success, 
               'Error en borrado físico: ' + ERROR_MESSAGE() AS Message;
    END CATCH
END;
GO

-- =============================================
-- PRUEBAS DEL SP
-- =============================================
/*
-- Test 1: Intentar borrar empleado CON pagos (debe fallar)
EXEC SP_DeleteEmployee_Physical @PersonaId = 152700726;

-- Test 2: Borrar empleado SIN pagos (debe funcionar)
EXEC SP_DeleteEmployee_Physical @PersonaId = 999999999;

-- Test 3: Intentar borrar empleado que no existe
EXEC SP_DeleteEmployee_Physical @PersonaId = 111111111;
*/