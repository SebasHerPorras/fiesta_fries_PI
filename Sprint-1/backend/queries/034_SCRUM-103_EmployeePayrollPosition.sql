USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

-- Tabla para almacenar el puesto asignado por empleado en una planilla
IF OBJECT_ID('dbo.EmployeePayrollPosition', 'U') IS NULL
BEGIN
    CREATE TABLE [Fiesta_Fries_DB].EmployeePayrollPosition (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ReportId INT NOT NULL,            -- FK a Payroll.PayrollId (opcional, a�adir FK si se desea)
        EmployeeId INT NOT NULL,          -- id de Empleado / Persona
        Position NVARCHAR(200) NOT NULL,  -- puesto con el que se contrat� / asign� para esa planilla
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );

    CREATE UNIQUE INDEX UX_EmployeePayrollPosition_Report_Employee ON [Fiesta_Fries_DB].[EmployeePayrollPosition](ReportId, EmployeeId);
END;
GO

USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

-- =============================================
-- SCRUM-54: Stored Procedure para obtener empleados para planilla
-- ACTUALIZADO: guarda el puesto en EmployeePayrollPosition si se pasa @ReportId
-- =============================================
/*
CREATE OR ALTER PROCEDURE SP_GetEmployeesForPayroll
    @CedulaJuridica BIGINT,
    @FechaInicio DATE,
    @FechaFin DATE,
    @ReportId INT = NULL      -- opcional: si se suministra, se guardar�/actualizar� el puesto en EmployeePayrollPosition
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        ------------------------------------------------
        -- 1) Seleccionar empleados en tabla temporal (incluye Position)
        ------------------------------------------------
        SELECT 
            p.id AS CedulaEmpleado,
            p.firstName + ' ' + p.secondName AS NombreEmpleado,
            -- Calcular salario seg�n tipo de empleado y frecuencia de pago
            CASE 
                WHEN LOWER(LTRIM(RTRIM(e.employmentType))) LIKE '%hora%' THEN 
                    e.salary * dbo.Fn_ObtenerHoras(e.id, @FechaInicio, @FechaFin)
                ELSE 
                    CASE 
                        WHEN LOWER(LTRIM(RTRIM(emp.FrecuenciaPago))) = 'quincenal' THEN e.salary / 2.0
                        WHEN LOWER(LTRIM(RTRIM(emp.FrecuenciaPago))) = 'semanal'   THEN e.salary / 4.0
                        WHEN LOWER(LTRIM(RTRIM(emp.FrecuenciaPago))) = 'mensual'   THEN e.salary
                        ELSE e.salary
                    END
            END AS SalarioBruto,
            e.employmentType AS TipoEmpleado,
            p.birthdate AS Cumpleanos,
            dbo.Fn_ObtenerHoras(e.id, @FechaInicio, @FechaFin) AS horas,
            ISNULL(e.position, '') AS Position,
            e.department AS Department
        INTO #TempEmployeesForPayroll
        FROM Empleado e
        INNER JOIN Persona p ON e.id = p.id
        INNER JOIN [User] u ON p.uniqueUser = u.PK_User
        INNER JOIN Empresa emp ON e.idCompny = emp.CedulaJuridica
        WHERE e.idCompny = @CedulaJuridica
          AND (e.IsDeleted = 0 OR e.IsDeleted IS NULL)
          AND (p.IsDeleted = 0 OR p.IsDeleted IS NULL)
          AND u.active = 1;

        ------------------------------------------------
        -- 2) Si se proporcion� @ReportId: upsert (merge) en EmployeePayrollPosition
        ------------------------------------------------
        IF @ReportId IS NOT NULL
        BEGIN
            MERGE INTO dbo.EmployeePayrollPosition AS target
            USING (
                SELECT 
                    @ReportId AS ReportId,
                    CedulaEmpleado AS EmployeeId,
                    Position
                FROM #TempEmployeesForPayroll
            ) AS src (ReportId, EmployeeId, Position)
            ON target.ReportId = src.ReportId AND target.EmployeeId = src.EmployeeId
            WHEN MATCHED THEN
                UPDATE SET Position = src.Position, CreatedDate = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (ReportId, EmployeeId, Position, CreatedDate)
                VALUES (src.ReportId, src.EmployeeId, src.Position, SYSUTCDATETIME());
        END

        ------------------------------------------------
        -- 3) Devolver result set (compatibilidad + incluye Position)
        ------------------------------------------------
        SELECT
            CedulaEmpleado,
            NombreEmpleado,
            SalarioBruto,
            TipoEmpleado,
            Cumpleanos,
            horas,
            Position
        FROM #TempEmployeesForPayroll
        ORDER BY Department, NombreEmpleado;

        DROP TABLE #TempEmployeesForPayroll;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

select * from EmployeePayrollPosition
*/
