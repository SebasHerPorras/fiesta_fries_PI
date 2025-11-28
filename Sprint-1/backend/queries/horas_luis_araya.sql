USE Fiesta_fries_DB;
GO

-- ============================================
-- HORAS TRABAJADAS PARA LUIS ARAYA CHAVES (1101234574)
-- Supervisor - Tiempo completo (40 horas/semana)
-- Periodos: 16/07/2025 - 15/08/2025 y 16/08/2025 - 15/09/2025
-- ============================================

-- PERIODO 1: 16/07/2025 - 15/08/2025 (4 semanas completas)

-- Semana del 14/07/2025 (lunes) - incluye 16 y 17 de julio
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-07-14', 1101234574, 16);

-- Semana del 21/07/2025
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-07-21', 1101234574, 40);

-- Semana del 28/07/2025
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-07-28', 1101234574, 40);

-- Semana del 04/08/2025
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-08-04', 1101234574, 40);

-- Semana del 11/08/2025 (incluye hasta 15 de agosto)
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-08-11', 1101234574, 40);

-- PERIODO 2: 16/08/2025 - 15/09/2025 (4 semanas completas)

-- Semana del 18/08/2025 (lunes, primera semana completa del periodo)
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-08-18', 1101234574, 40);

-- Semana del 25/08/2025
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-08-25', 1101234574, 40);

-- Semana del 01/09/2025
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-09-01', 1101234574, 40);

-- Semana del 08/09/2025 (incluye hasta 15 de septiembre)
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-09-08', 1101234574, 40);

GO

-- ============================================
-- VERIFICACIONES
-- ============================================

-- Verificar las horas registradas para Luis Araya
SELECT 
    s.start_date AS FechaInicio,
    s.hours_count AS HorasSemana,
    p.firstName + ' ' + p.secondName AS Empleado
FROM Semana s
JOIN Persona p ON s.id_employee = p.id
WHERE s.id_employee = 1101234574
ORDER BY s.start_date;

-- Verificar total de horas por periodo
SELECT 
    'Periodo 16/07/2025 - 15/08/2025' AS Periodo,
    dbo.Fn_ObtenerHoras(1101234574, '2025-07-16', '2025-08-15') AS TotalHoras
UNION ALL
SELECT 
    'Periodo 16/08/2025 - 15/09/2025' AS Periodo,
    dbo.Fn_ObtenerHoras(1101234574, '2025-08-16', '2025-09-15') AS TotalHoras;
