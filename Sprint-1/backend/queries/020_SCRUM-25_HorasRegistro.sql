use Fiesta_Fries_DB;

create Table Semana(
[start_date] DATE NOT NULL,
id_employee int NOT NULL,
hours_count int DEFAULT 0,
CONSTRAINT pk_semana PRIMARY KEY (start_date, id_employee),
CONSTRAINT fk_week_employee FOREIGN KEY (id_employee) REFERENCES Empleado(id)
);

create Table Dia(
 [date] DATE NOT NULL,
 hours_count int DEFAULT 0,
 week_start_date DATE,
 id_employee INT NOT NULL,
 CONSTRAINT pk_dia PRIMARY KEY ([date], week_start_date, id_employee),
 CONSTRAINT fk_dia_week FOREIGN KEY (week_start_date, id_employee) REFERENCES Semana(start_date, id_employee)
);

CREATE PROCEDURE sp_GetOrCreateWeek
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
        INSERT INTO Semana(start_date, id_employee, hours_count)
        VALUES(@start_date, @id_employee, 0);

        SELECT * 
        FROM Semana 
        WHERE start_date = @start_date AND id_employee = @id_employee;
    END
END;
GO

CREATE PROCEDURE sp_GetOrCreateDay
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
        INSERT INTO Dia([date], week_start_date, id_employee, hours_count)
        VALUES (@date, @week_start_date, @id_employee, 0);

        SELECT *
        FROM Dia
        WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;
    END
END;
GO

CREATE PROCEDURE sp_AddHoursToDay
    @hours_count INT,
    @date DATE,
    @week_start_date DATE,
    @id_employee INT
AS
BEGIN
    UPDATE Dia
    SET hours_count = hours_count + @hours_count
    WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;
END;
GO


CREATE PROCEDURE sp_AddHoursToDayAndGetTable
    @hours_count INT,
    @date DATE,
    @week_start_date DATE,
    @id_employee INT
AS
BEGIN
    UPDATE Dia
    SET hours_count = hours_count + @hours_count
    WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;

    SELECT *
    FROM Dia
    WHERE [date] = @date AND week_start_date = @week_start_date AND id_employee = @id_employee;
END;
GO

CREATE TRIGGER trg_UpdateWeekHours
ON Dia
AFTER INSERT, UPDATE
AS
BEGIN
    UPDATE s
    SET s.hours_count = (
        SELECT SUM(d.hours_count)
        FROM Dia d
        WHERE d.week_start_date = s.start_date
          AND d.id_employee = s.id_employee
    )
    FROM Semana s
    INNER JOIN inserted i 
        ON s.start_date = i.week_start_date 
       AND s.id_employee = i.id_employee;
END;
GO


Select* from Semana where[start_date] = '2025-07-31'; 
Select* from Día where  [date] = '2025-07-31';