USE Fiesta_fries_DB;
GO


-- insertar un empleador
INSERT INTO [User] (email, password, active, admin) VALUES 
('sebas@gmail.com', 'Hola.2025', 1, 0),
('emilio@gmail.com', 'Hola.2025', 1, 0),
('enrique@gmail.com', 'Hola.2025', 1, 0),
('nacho@gmail.com', 'Hola.2025', 1, 0);

INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1, 'Sebastian', 'Hernandez Porras', '2000-05-15', 'Calle 123, Ciudad', '8888-8888', '2222-2222', (SELECT PK_User FROM [User] WHERE email = 'sebas@gmail.com'), 'Empleador')

DECLARE @personaId INT;
DECLARE @userId UNIQUEIDENTIFIER;

-- 1 Obtener el PK_User del email
SET @userId = (SELECT PK_User 
               FROM [User] 
               WHERE email = 'sebas@gmail.com');

-- 2 Obtener el id de Persona que corresponde a ese usuario
SET @personaId = (SELECT id 
                  FROM Persona 
                  WHERE uniqueUser = @userId);

-- 2 Insertar en Empresa usando ese id
INSERT INTO Empresa (CedulaJuridica, Nombre, DueñoEmpresa, Telefono, DireccionEspecifica, NoMaxBeneficios, DiaPago, FrecuenciaPago)
VALUES 
(3101123456, 'Fiesta Fries', @personaId, 22222222, 'Avenida Central, San Jose', 100, 15, 'Mensual'),
(3102234567, 'Taco Bell', @personaId, 33333333, 'Boulevard de Rohrmoser, San Jose', 150, 30, 'Quincenal');

SELECT 
    e.CedulaJuridica,
    e.Nombre AS NombreEmpresa,
    e.Telefono,
    e.DireccionEspecifica,
    e.NoMaxBeneficios,
    e.DiaPago,
    e.FrecuenciaPago,
    p.firstName + ' ' + p.secondName AS NombrePersona,
    u.email
FROM Empresa e
JOIN Persona p ON e.DueñoEmpresa = p.id
JOIN [User] u ON p.uniqueUser = u.PK_User
WHERE u.email = 'sebas@gmail.com';


select * from Empresa;
select * from Persona;
select * from [User];


INSERT INTO [User] (email, password) VALUES 
('sebastian.hernandezporras@ucr.ac.cr', 'password123'),
('emilio.romero@ucr.ac.cr', 'password123'),
('diego.cerdasdelgado@ucr.ac.cr', 'password123'),
('enrique.bermudez@ucr.ac.cr', 'password123');

UPDATE [User]
SET [password] = 'Hola.2025'
WHERE email = 'sebastian.hernandezporras@ucr.ac.cr';

UPDATE [User]
SET [password] = 'Hola.2025'
WHERE email = 'emilio.romero@ucr.ac.cr';

UPDATE [User]
SET [password] = 'Hola.2025'
WHERE email = 'diego.cerdasdelgado@ucr.ac.cr';

UPDATE [User]
SET [password] = 'Hola.2025'
WHERE email = 'enrique.bermudez@ucr.ac.cr';

update [User]
set active = 1
where email = 'sebastian.hernandezporras@ucr.ac.cr'

update  [User] 
set [admin] = 1
where email = 'sebastian.hernandezporras@ucr.ac.cr';

-- Verificar que los usuarios est�n activos
UPDATE [User] SET active = 1 
WHERE email IN ('diego.cerdasdelgado@ucr.ac.cr', 'emilio.romero@ucr.ac.cr', 'enrique.bermudez@ucr.ac.cr');


INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta)
VALUES (
    3101123456,
    'Seguro de coche',
    'Porcentual',
    'Empresa',
    10.50,
    'Beneficio'
);
select * from Beneficio

-- Insertar empleados para Taco Bell (CedulaJuridica: 3102234567)

-- EMPLEADO 1: Diego Cerdas
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(119180741, 'Diego', 'Cerdas Delgado', '1995-03-10', 'Cartago, Costa Rica', '8888-1234', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'diego.cerdasdelgado@ucr.ac.cr'), 'Empleado');

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(119180741, 'Cocinero', 'Tiempo completo', 450000, '2024-01-15', 'Cocina', 3102234567);

-- EMPLEADO 2: Emilio Romero  
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(205670889, 'Emilio', 'Romero Vargas', '1992-07-22', 'San Jos�, Costa Rica', '8777-5678', '2444-5678', 
 (SELECT PK_User FROM [User] WHERE email = 'emilio.romero@ucr.ac.cr'), 'Empleado');

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(205670889, 'Cajero', 'Medio tiempo', 320000, '2024-02-01', 'Ventas', 3102234567);

-- EMPLEADO 3: Enrique Berm�dez
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(304560123, 'Enrique', 'Berm�dez L�pez', '1988-11-15', 'Heredia, Costa Rica', '8555-9012', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'enrique.bermudez@ucr.ac.cr'), 'Empleado');

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(304560123, 'Supervisor', 'Tiempo completo', 600000, '2023-12-01', 'Administraci�n', 3102234567);



-- Empleados adicionales para Taco Bell

-- EMPLEADO 4: Laura Jiménez
INSERT INTO [User] (email, password, active) VALUES 
('laura.jimenez@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(401234567, 'Laura', 'Jiménez Soto', '1997-08-21', 'Alajuela, Costa Rica', '8999-4567', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'laura.jimenez@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(401234567, 'Mesera', 'Medio tiempo', 280000, '2024-03-05', 'Servicio', 3102234567);

-- EMPLEADO 5: Andrés Mora
INSERT INTO [User] (email, password, active) VALUES 
('andres.mora@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(501234568, 'Andrés', 'Mora Salas', '1994-12-02', 'San José, Costa Rica', '8888-5678', '2222-3333', 
 (SELECT PK_User FROM [User] WHERE email = 'andres.mora@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(501234568, 'Repartidor', 'Tiempo completo', 350000, '2024-04-10', 'Logística', 3102234567);

-- EMPLEADO 6: Mariana Rojas
INSERT INTO [User] (email, password, active) VALUES 
('mariana.rojas@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(601234569, 'Mariana', 'Rojas Fernández', '1999-06-18', 'Heredia, Costa Rica', '8777-1234', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'mariana.rojas@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(601234569, 'Cocinera', 'Tiempo completo', 420000, '2024-05-01', 'Cocina', 3102234567);

-- EMPLEADO 7: Pablo Castillo
INSERT INTO [User] (email, password, active) VALUES 
('pablo.castillo@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(701234570, 'Pablo', 'Castillo Vargas', '1996-09-30', 'Cartago, Costa Rica', '8666-7890', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'pablo.castillo@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(701234570, 'Limpieza', 'Medio tiempo', 200000, '2024-06-15', 'Mantenimiento', 3102234567);

-- EMPLEADO 8: Sofía Navarro
INSERT INTO [User] (email, password, active) VALUES 
('sofia.navarro@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(801234571, 'Sofía', 'Navarro Jiménez', '1998-04-25', 'Puntarenas, Costa Rica', '8555-4321', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'sofia.navarro@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(801234571, 'Cajera', 'Tiempo completo', 330000, '2024-07-01', 'Ventas', 3102234567);

-- Empleados para Fiesta Fries

-- EMPLEADO 1: Carlos Méndez
INSERT INTO [User] (email, password, active) VALUES 
('carlos.mendez@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(901234572, 'Carlos', 'Méndez Solís', '1993-11-11', 'San José, Costa Rica', '8999-8765', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'carlos.mendez@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(901234572, 'Cocinero', 'Tiempo completo', 410000, '2024-03-20', 'Cocina', 3101123456);

-- EMPLEADO 2: Ana Salazar
INSERT INTO [User] (email, password, active) VALUES 
('ana.salazar@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1001234573, 'Ana', 'Salazar Rojas', '1995-02-14', 'Heredia, Costa Rica', '8777-6543', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'ana.salazar@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1001234573, 'Cajera', 'Medio tiempo', 250000, '2024-04-05', 'Ventas', 3101123456);

-- EMPLEADO 3: Luis Araya
INSERT INTO [User] (email, password, active) VALUES 
('luis.araya@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1101234574, 'Luis', 'Araya Chaves', '1990-07-30', 'Alajuela, Costa Rica', '8666-3210', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'luis.araya@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1101234574, 'Supervisor', 'Tiempo completo', 550000, '2024-05-10', 'Administración', 3101123456);



-- Beneficios para Fiesta Fries
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta)
VALUES 
(3101123456, 'Seguro Médico', 'Porcentual', 'Empresa', 8.00, 'Beneficio'),
(3101123456, 'Transporte', 'API', 'Empresa', 15000, 'Beneficio'),
(3101123456, 'Almuerzo', 'Monto Fijo', 'Empresa', 5000, 'Beneficio'),
(3101123456, 'Capacitación', 'Monto Fijo', 'Empresa', 10000, 'Beneficio');

-- Beneficios para Taco Bell
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta)
VALUES 
(3102234567, 'Seguro de Vida', 'Porcentual', 'Empresa', 12.00, 'Beneficio'),
(3102234567, 'Bonificación', 'Monto Fijo', 'Empresa', 20000, 'Beneficio'),
(3102234567, 'Uniforme', 'API', 'Empresa', 3000, 'Beneficio'),
(3102234567, 'Almuerzo', 'Monto Fijo', 'Empresa', 5000, 'Beneficio'),
(3102234567, 'Capacitación', 'Monto Fijo', 'Empresa', 10000, 'Beneficio');

-- 10 EMPLEADOS ADICIONALES PARA TACO BELL

-- EMPLEADO 9: Gabriela Herrera
INSERT INTO [User] (email, password, active) VALUES 
('gabriela.herrera@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1001234575, 'Gabriela', 'Herrera Vega', '1996-01-12', 'San José, Costa Rica', '8888-9876', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'gabriela.herrera@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1001234575, 'Hostess', 'Medio tiempo', 290000, '2024-08-01', 'Servicio', 3102234567);

-- EMPLEADO 10: Roberto Silva
INSERT INTO [User] (email, password, active) VALUES 
('roberto.silva@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1101234576, 'Roberto', 'Silva Montero', '1991-05-18', 'Cartago, Costa Rica', '8777-4567', '2333-7890', 
 (SELECT PK_User FROM [User] WHERE email = 'roberto.silva@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1101234576, 'Gerente de Turno', 'Tiempo completo', 650000, '2024-01-10', 'Administración', 3102234567);

-- EMPLEADO 11: Melissa Arias
INSERT INTO [User] (email, password, active) VALUES 
('melissa.arias@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1201234577, 'Melissa', 'Arias Cordero', '1998-09-03', 'Alajuela, Costa Rica', '8999-3456', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'melissa.arias@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1201234577, 'Preparadora de Alimentos', 'Tiempo completo', 380000, '2024-02-15', 'Cocina', 3102234567);

-- EMPLEADO 12: Javier Ramírez
INSERT INTO [User] (email, password, active) VALUES 
('javier.ramirez@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1301234578, 'Javier', 'Ramírez Castro', '1994-12-25', 'Heredia, Costa Rica', '8666-5432', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'javier.ramirez@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1301234578, 'Cajero', 'Medio tiempo', 315000, '2024-03-12', 'Ventas', 3102234567);

-- EMPLEADO 13: Carmen López
INSERT INTO [User] (email, password, active) VALUES 
('carmen.lopez@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1401234579, 'Carmen', 'López Gutierrez', '1992-06-08', 'San José, Costa Rica', '8555-6789', '2444-1122', 
 (SELECT PK_User FROM [User] WHERE email = 'carmen.lopez@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1401234579, 'Encargada de Inventario', 'Tiempo completo', 480000, '2024-04-20', 'Logística', 3102234567);

-- EMPLEADO 14: Alejandro Mora
INSERT INTO [User] (email, password, active) VALUES 
('alejandro.mora@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1501234580, 'Alejandro', 'Mora Picado', '1997-03-14', 'Puntarenas, Costa Rica', '8888-7654', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'alejandro.mora@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1501234580, 'Repartidor', 'Tiempo completo', 360000, '2024-05-05', 'Logística', 3102234567);

-- EMPLEADO 15: Patricia Vásquez
INSERT INTO [User] (email, password, active) VALUES 
('patricia.vasquez@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1601234581, 'Patricia', 'Vásquez Alvarado', '1995-10-29', 'Cartago, Costa Rica', '8777-8901', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'patricia.vasquez@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1601234581, 'Mesera', 'Medio tiempo', 275000, '2024-06-10', 'Servicio', 3102234567);

-- EMPLEADO 16: Fernando Solís
INSERT INTO [User] (email, password, active) VALUES 
('fernando.solis@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1701234582, 'Fernando', 'Solís Chaves', '1993-08-17', 'Alajuela, Costa Rica', '8999-2345', '2555-6677', 
 (SELECT PK_User FROM [User] WHERE email = 'fernando.solis@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1701234582, 'Cocinero', 'Tiempo completo', 430000, '2024-07-18', 'Cocina', 3102234567);

-- EMPLEADO 17: Natalia Rojas
INSERT INTO [User] (email, password, active) VALUES 
('natalia.rojas@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1801234583, 'Natalia', 'Rojas Quesada', '1999-02-28', 'Heredia, Costa Rica', '8666-1357', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'natalia.rojas@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1801234583, 'Cajera', 'Medio tiempo', 300000, '2024-08-25', 'Ventas', 3102234567);

-- EMPLEADO 18: Ricardo Jiménez
INSERT INTO [User] (email, password, active) VALUES 
('ricardo.jimenez@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1901234584, 'Ricardo', 'Jiménez Vargas', '1990-11-05', 'San José, Costa Rica', '8555-9876', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'ricardo.jimenez@ucr.ac.cr'), 'Empleado');
INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1901234584, 'Supervisor de Limpieza', 'Tiempo completo', 390000, '2024-09-01', 'Mantenimiento', 3102234567);


select * from Beneficio

-- Verificar empleados de Taco Bell
SELECT 
    e.id AS EmpleadoID,
    p.firstName + ' ' + p.secondName AS NombreCompleto,
    e.position AS Posicion,
    e.employmentType AS TipoEmpleo,
    e.salary AS Salario,
    e.hireDate AS FechaIngreso,
    e.department AS Departamento,
    emp.Nombre AS Empresa,
    u.email AS Email
FROM Empleado e
JOIN Persona p ON e.id = p.id
JOIN [User] u ON p.uniqueUser = u.PK_User
JOIN Empresa emp ON e.idCompny = emp.CedulaJuridica
WHERE e.idCompny = 3102234567
ORDER BY e.hireDate;
