USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB

CREATE TABLE [Fiesta_Fries_DB].[Semana](
[start_date] DATE NOT NULL,
id_employee int NOT NULL,
hours_count int DEFAULT 0,
CONSTRAINT pk_semana PRIMARY KEY (start_date, id_employee),
CONSTRAINT fk_week_employee FOREIGN KEY (id_employee) REFERENCES Empleado(id)
);

CREATE TABLE [Fiesta_Fries_DB].[Dia](
 [date] DATE NOT NULL,
 hours_count int DEFAULT 0,
 week_start_date DATE,
 id_employee INT NOT NULL,
 CONSTRAINT pk_dia PRIMARY KEY ([date], week_start_date, id_employee),
 CONSTRAINT fk_dia_week FOREIGN KEY (week_start_date, id_employee) REFERENCES Semana(start_date, id_employee)
);

GO
CREATE PROCEDURE [Fiesta_Fries_DB].sp_GetOrCreateWeek
    @start_date DATE,
    @id_employee INT
AS
BEGIN
    IF EXISTS(
        SELECT 1
        FROM Semana
        WHERE start_date = @start_date AND id_employee = @id_employee
    )
    BEGIN
        SELECT * 
        FROM Semana 
        WHERE start_date = @start_date AND id_employee = @id_employee;
    END
    ELSE
    BEGIN
        INSERT INTO [Fiesta_Fries_DB].[Semana](start_date, id_employee, hours_count)
        VALUES(@start_date, @id_employee, 0);

        SELECT * 
        FROM Semana 
        WHERE start_date = @start_date AND id_employee = @id_employee;
    END
END;
GO

CREATE PROCEDURE [Fiesta_Fries_DB].sp_GetOrCreateDay
    @date DATE,
    @week_start_date DATE,
    @id_employee INT
AS
BEGIN
    IF EXISTS(
        SELECT 1
        FROM Dia
        WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee
    )
    BEGIN
        SELECT *
        FROM Dia
        WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;
    END
    ELSE
    BEGIN
        INSERT INTO [Fiesta_Fries_DB].[Dia]([date], week_start_date, id_employee, hours_count)
        VALUES (@date, @week_start_date, @id_employee, 0);

        SELECT *
        FROM Dia
        WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;
    END
END;
GO

CREATE PROCEDURE [Fiesta_Fries_DB].sp_AddHoursToDay
    @hours_count INT,
    @date DATE,
    @week_start_date DATE,
    @id_employee INT
AS
BEGIN
    UPDATE [Fiesta_Fries_DB].[Dia]
    SET hours_count = hours_count + @hours_count
    WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;
END;
GO


CREATE PROCEDURE [Fiesta_Fries_DB].sp_AddHoursToDayAndGetTable
    @hours_count INT,
    @date DATE,
    @week_start_date DATE,
    @id_employee INT
AS
BEGIN
    UPDATE [Fiesta_Fries_DB].[Dia]
    SET hours_count = hours_count + @hours_count
    WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;

    SELECT *
    FROM Dia
    WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;
END;
GO

CREATE FUNCTION [Fiesta_Fries_DB].Fn_ObtenerHoras(@id_employee int,@start_date DATE, @end_date DATE)
RETURNS INT
AS 
BEGIN
DECLARE @hours_total INT;
SELECT @hours_total = ISNULL(SUM(ISNULL(hours_count,0)), 0)
FROM Semana WHERE [start_date] BETWEEN @start_date AND @end_date 
    AND id_employee = @id_employee;

    RETURN ISNULL(@hours_total,0);
END;
GO

SELECT [Fiesta_Fries_DB].Fn_ObtenerHoras(119180745,'2025-10-1','2025-10-31') AS horas_semana;


--



