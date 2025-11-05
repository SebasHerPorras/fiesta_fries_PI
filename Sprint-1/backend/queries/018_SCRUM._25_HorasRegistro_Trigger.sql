USE Fiesta_Fries_DB;

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