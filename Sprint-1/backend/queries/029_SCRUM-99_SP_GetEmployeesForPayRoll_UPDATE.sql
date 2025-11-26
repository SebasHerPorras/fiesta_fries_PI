USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

-- =============================================
-- SCRUM-54: Stored Procedure para obtener empleados para planilla
-- =============================================
-- ACTUALIZADO SCRUM-99: Filtrar empleados eliminados (IsDeleted)
-- =============================================

CREATE OR ALTER PROCEDURE [Fiesta_Fries_DB].SP_GetEmployeesForPayroll
    @CedulaJuridica BIGINT,
    @FechaInicio DATE,
    @FechaFin DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.id AS CedulaEmpleado,
        p.firstName + ' ' + p.secondName AS NombreEmpleado,
        -- Calcular salario seg�n tipo de empleado y frecuencia de pago
        CASE 
            -- Por horas: salario base (tarifa por hora) * horas trabajadas
            WHEN LOWER(LTRIM(RTRIM(e.employmentType))) LIKE '%hora%' THEN 
                e.salary * dbo.Fn_ObtenerHoras(e.id, @FechaInicio, @FechaFin)
                
            -- Empleados regulares: ajustar seg�n frecuencia de pago de la empresa
            ELSE 
                CASE 
                    -- Pago quincenal: salario / 2
                    WHEN LOWER(LTRIM(RTRIM(emp.FrecuenciaPago))) = 'quincenal' THEN 
                        e.salary / 2.0
                    -- Pago semanal: salario / 4
                    WHEN LOWER(LTRIM(RTRIM(emp.FrecuenciaPago))) = 'semanal' THEN 
                        e.salary / 4.0
                    -- Pago mensual: salario completo
                    WHEN LOWER(LTRIM(RTRIM(emp.FrecuenciaPago))) = 'mensual' THEN 
                        e.salary
                    -- Por defecto: salario completo
                    ELSE 
                        e.salary
                END
        END AS SalarioBruto,
        e.employmentType AS TipoEmpleado,
        p.birthdate AS Cumpleanos,
        dbo.Fn_ObtenerHoras(e.id, @FechaInicio, @FechaFin) AS horas
    FROM Empleado e
    INNER JOIN Persona p ON e.id = p.id
    INNER JOIN [User] u ON p.uniqueUser = u.PK_User
    INNER JOIN Empresa emp ON e.idCompny = emp.CedulaJuridica
    WHERE e.idCompny = @CedulaJuridica
      --  Filtrar empleados eliminados
      AND (e.IsDeleted = 0 OR e.IsDeleted IS NULL)
      AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
      --  Filtrar usuarios activos
      AND u.active = 1
    ORDER BY e.department, p.firstName;
END;
GO

-- =============================================
-- PRUEBA DEL SP
-- =============================================
/*
-- Ejecutar para empresa de prueba
EXEC SP_GetEmployeesForPayroll 
    @CedulaJuridica = 550020002,
    @FechaInicio = '2025-10-01',
    @FechaFin = '2025-10-31';
*/