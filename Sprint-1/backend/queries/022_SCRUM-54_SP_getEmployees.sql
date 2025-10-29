use Fiesta_Fries_DB;
GO


-- Stored Procedure para obtener empleados con salario calculado para planilla
CREATE OR ALTER PROCEDURE SP_GetEmployeesForPayroll
    @CedulaJuridica BIGINT,
    @FechaInicio DATE,
    @FechaFin DATE
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        p.id AS CedulaEmpleado,
        p.firstName + ' ' + p.secondName AS NombreEmpleado,
        -- Calcular salario según tipo de empleado
        CASE 
            -- Por horas: salario base (tarifa por hora) * horas trabajadas
            WHEN LOWER(LTRIM(RTRIM(e.employmentType))) = 'por horas' THEN 
                e.salary * dbo.Fn_ObtenerHoras(e.id, @FechaInicio, @FechaFin)
                
            -- Medio tiempo: salario base / 2
            WHEN LOWER(LTRIM(RTRIM(e.employmentType))) = 'medio tiempo' THEN 
                e.salary / 2.0
                
            -- Tiempo completo: salario base completo
            WHEN LOWER(LTRIM(RTRIM(e.employmentType))) = 'tiempo completo' THEN 
                e.salary
                
            -- Por defecto: salario base
            ELSE 
                e.salary
        END AS SalarioBruto,
        e.employmentType AS TipoEmpleado,
        p.birthdate AS Cumpleanos,
        dbo.Fn_ObtenerHoras(e.id, @FechaInicio, @FechaFin) AS horas
    FROM Empleado e
    INNER JOIN Persona p ON e.id = p.id
    INNER JOIN [User] u ON p.uniqueUser = u.PK_User
    WHERE e.idCompny = @CedulaJuridica
    ORDER BY e.department, p.firstName;
END;
GO