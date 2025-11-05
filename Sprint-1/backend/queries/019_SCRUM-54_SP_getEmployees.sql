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
        -- Calcular salario según tipo de empleado y frecuencia de pago
        CASE 
            -- Por horas: salario base (tarifa por hora) * horas trabajadas
            WHEN LOWER(LTRIM(RTRIM(e.employmentType))) = 'Por Horas' THEN 
                e.salary * dbo.Fn_ObtenerHoras(e.id, @FechaInicio, @FechaFin)
                
            -- Empleados regulares: ajustar según frecuencia de pago de la empresa
            ELSE 
                CASE 
                    -- Pago quincenal: salario / 2
                    WHEN LOWER(LTRIM(RTRIM(emp.FrecuenciaPago))) = 'Quincenal' THEN 
                        e.salary / 2.0
                    -- Pago mensual: salario completo
                    WHEN LOWER(LTRIM(RTRIM(emp.FrecuenciaPago))) = 'Mensual' THEN 
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
    ORDER BY e.department, p.firstName;
END;
GO