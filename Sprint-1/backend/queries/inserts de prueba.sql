USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO


-- insertar un empleador
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active, admin) VALUES 
('sebas@gmail.com', 'Hola.2025', 1, 0),
('emilio@gmail.com', 'Hola.2025', 1, 0),
('enrique@gmail.com', 'Hola.2025', 1, 0),
('nacho@gmail.com', 'Hola.2025', 1, 0);

INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
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
INSERT INTO [Fiesta_Fries_DB].[Empresa] (CedulaJuridica, Nombre, DueñoEmpresa, Telefono, DireccionEspecifica, NoMaxBeneficios, DiaPago, FrecuenciaPago)
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


select * FROM [Fiesta_Fries_DB].[Persona];
select * FROM [Fiesta_Fries_DB].[Persona];
select * FROM [Fiesta_Fries_DB].[Persona];


INSERT INTO [Fiesta_Fries_DB].[User] (email, password) VALUES 
('sebastian.hernandezporras@ucr.ac.cr', 'password123'),
('emilio.romero@ucr.ac.cr', 'password123'),
('diego.cerdasdelgado@ucr.ac.cr', 'password123'),
('enrique.bermudez@ucr.ac.cr', 'password123');

UPDATE [Fiesta_Fries_DB].[Empresa] SET [password] = 'Hola.2025'
WHERE email = 'sebastian.hernandezporras@ucr.ac.cr';

UPDATE [Fiesta_Fries_DB].[Empresa] SET [password] = 'Hola.2025'
WHERE email = 'emilio.romero@ucr.ac.cr';

UPDATE [Fiesta_Fries_DB].[Empresa] SET [password] = 'Hola.2025'
WHERE email = 'diego.cerdasdelgado@ucr.ac.cr';

UPDATE [Fiesta_Fries_DB].[Empresa] SET [password] = 'Hola.2025'
WHERE email = 'enrique.bermudez@ucr.ac.cr';

UPDATE [Fiesta_Fries_DB].[Empresa] SET active = 1
where email = 'sebastian.hernandezporras@ucr.ac.cr'

update  [User] 
set [admin] = 1
where email = 'sebastian.hernandezporras@ucr.ac.cr';

-- Verificar que los usuarios est?n activos
UPDATE [Fiesta_Fries_DB].[Empresa] SET active = 1 
WHERE email IN ('diego.cerdasdelgado@ucr.ac.cr', 'emilio.romero@ucr.ac.cr', 'enrique.bermudez@ucr.ac.cr');


INSERT INTO [Fiesta_Fries_DB].[Empresa] (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta)
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
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(119180741, 'Diego', 'Cerdas Delgado', '1995-03-10', 'Cartago, Costa Rica', '8888-1234', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'diego.cerdasdelgado@ucr.ac.cr'), 'Empleado');

INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(119180741, 'Cocinero', 'Tiempo completo', 450000, '2024-01-15', 'Cocina', 3102234567);

-- EMPLEADO 2: Emilio Romero  
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(205670889, 'Emilio', 'Romero Vargas', '1992-07-22', 'San Jos?, Costa Rica', '8777-5678', '2444-5678', 
 (SELECT PK_User FROM [User] WHERE email = 'emilio.romero@ucr.ac.cr'), 'Empleado');

INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(205670889, 'Cajero', 'Medio tiempo', 320000, '2024-02-01', 'Ventas', 3102234567);

-- EMPLEADO 3: Enrique Berm?dez
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(304560123, 'Enrique', 'Berm?dez L?pez', '1988-11-15', 'Heredia, Costa Rica', '8555-9012', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'enrique.bermudez@ucr.ac.cr'), 'Empleado');

INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(304560123, 'Supervisor', 'Tiempo completo', 600000, '2023-12-01', 'Administraci?n', 3102234567);



-- Empleados adicionales para Taco Bell

-- EMPLEADO 4: Laura Jiménez
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active) VALUES 
('laura.jimenez@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(401234567, 'Laura', 'Jiménez Soto', '1997-08-21', 'Alajuela, Costa Rica', '8999-4567', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'laura.jimenez@ucr.ac.cr'), 'Empleado');
INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(401234567, 'Mesera', 'Medio tiempo', 280000, '2024-03-05', 'Servicio', 3102234567);

-- EMPLEADO 5: Andrés Mora
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active) VALUES 
('andres.mora@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(501234568, 'Andrés', 'Mora Salas', '1994-12-02', 'San José, Costa Rica', '8888-5678', '2222-3333', 
 (SELECT PK_User FROM [User] WHERE email = 'andres.mora@ucr.ac.cr'), 'Empleado');
INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(501234568, 'Repartidor', 'Tiempo completo', 350000, '2024-04-10', 'Logística', 3102234567);

-- EMPLEADO 6: Mariana Rojas
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active) VALUES 
('mariana.rojas@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(601234569, 'Mariana', 'Rojas Fernández', '1999-06-18', 'Heredia, Costa Rica', '8777-1234', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'mariana.rojas@ucr.ac.cr'), 'Empleado');
INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(601234569, 'Cocinera', 'Tiempo completo', 420000, '2024-05-01', 'Cocina', 3102234567);

-- EMPLEADO 7: Pablo Castillo
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active) VALUES 
('pablo.castillo@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(701234570, 'Pablo', 'Castillo Vargas', '1996-09-30', 'Cartago, Costa Rica', '8666-7890', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'pablo.castillo@ucr.ac.cr'), 'Empleado');
INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(701234570, 'Limpieza', 'Medio tiempo', 200000, '2024-06-15', 'Mantenimiento', 3102234567);

-- EMPLEADO 8: Sofía Navarro
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active) VALUES 
('sofia.navarro@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(801234571, 'Sofía', 'Navarro Jiménez', '1998-04-25', 'Puntarenas, Costa Rica', '8555-4321', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'sofia.navarro@ucr.ac.cr'), 'Empleado');
INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(801234571, 'Cajera', 'Tiempo completo', 330000, '2024-07-01', 'Ventas', 3102234567);

-- Empleados para Fiesta Fries

-- EMPLEADO 1: Carlos Méndez
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active) VALUES 
('carlos.mendez@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(901234572, 'Carlos', 'Méndez Solís', '1993-11-11', 'San José, Costa Rica', '8999-8765', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'carlos.mendez@ucr.ac.cr'), 'Empleado');
INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(901234572, 'Cocinero', 'Tiempo completo', 410000, '2024-03-20', 'Cocina', 3101123456);

-- EMPLEADO 2: Ana Salazar
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active) VALUES 
('ana.salazar@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1001234573, 'Ana', 'Salazar Rojas', '1995-02-14', 'Heredia, Costa Rica', '8777-6543', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'ana.salazar@ucr.ac.cr'), 'Empleado');
INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1001234573, 'Cajera', 'Medio tiempo', 250000, '2024-04-05', 'Ventas', 3101123456);

-- EMPLEADO 3: Luis Araya
INSERT INTO [Fiesta_Fries_DB].[User] (email, password, active) VALUES 
('luis.araya@ucr.ac.cr', 'Hola.2025', 1);
INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(1101234574, 'Luis', 'Araya Chaves', '1990-07-30', 'Alajuela, Costa Rica', '8666-3210', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'luis.araya@ucr.ac.cr'), 'Empleado');
INSERT INTO [Fiesta_Fries_DB].[Empleado] (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(1101234574, 'Supervisor', 'Tiempo completo', 550000, '2024-05-10', 'Administración', 3101123456);



-- Beneficios para Fiesta Fries
INSERT INTO [Fiesta_Fries_DB].[Empresa] (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta)
VALUES 
(3101123456, 'Seguro Médico', 'Porcentual', 'Empresa', 8.00, 'Beneficio'),
(3101123456, 'Transporte', 'API', 'Empresa', 15000, 'Beneficio'),
(3101123456, 'Almuerzo', 'Monto Fijo', 'Empresa', 5000, 'Beneficio'),
(3101123456, 'Capacitación', 'Monto Fijo', 'Empresa', 10000, 'Beneficio');

-- Beneficios para Taco Bell
INSERT INTO [Fiesta_Fries_DB].[Empresa] (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta)
VALUES 
(3102234567, 'Seguro de Vida', 'Porcentual', 'Empresa', 12.00, 'Beneficio'),
(3102234567, 'Bonificación', 'Monto Fijo', 'Empresa', 20000, 'Beneficio'),
(3102234567, 'Uniforme', 'API', 'Empresa', 3000, 'Beneficio'),
(3102234567, 'Almuerzo', 'Monto Fijo', 'Empresa', 5000, 'Beneficio'),
(3102234567, 'Capacitación', 'Monto Fijo', 'Empresa', 10000, 'Beneficio');

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
