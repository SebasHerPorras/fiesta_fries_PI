USE Fiesta_fries_DB;
GO


-- insertar un empleador
INSERT INTO [User] (email, password, active, admin) VALUES 
('sebas@gmail.com', 'Hola.2025', 1, 0)

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

-- Verificar que los usuarios estén activos
UPDATE [User] SET active = 1 
WHERE email IN ('diego.cerdasdelgado@ucr.ac.cr', 'emilio.romero@ucr.ac.cr', 'enrique.bermudez@ucr.ac.cr');



---- Quitar las FOREIGN KEYS
--ALTER TABLE dbo.Persona DROP CONSTRAINT FK_Persona_Usuario;
--ALTER TABLE dbo.Empresa DROP CONSTRAINT FK_Empresa_Persona;
--ALTER TABLE dbo.Empleado DROP CONSTRAINT FK_Empleado_Persona; -- si existe

---- Ahora sí podés borrar tablas
--DROP TABLE IF EXISTS dbo.Empresa;
--DROP TABLE IF EXISTS dbo.Empleado;
--DROP TABLE IF EXISTS dbo.Persona;
--DROP TABLE IF EXISTS dbo.[User];


INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta)
VALUES (
    3101123456,
    'Seguro de Vida',
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
(205670889, 'Emilio', 'Romero Vargas', '1992-07-22', 'San José, Costa Rica', '8777-5678', '2444-5678', 
 (SELECT PK_User FROM [User] WHERE email = 'emilio.romero@ucr.ac.cr'), 'Empleado');

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(205670889, 'Cajero', 'Medio tiempo', 320000, '2024-02-01', 'Ventas', 3102234567);

-- EMPLEADO 3: Enrique Bermúdez
INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType) VALUES 
(304560123, 'Enrique', 'Bermúdez López', '1988-11-15', 'Heredia, Costa Rica', '8555-9012', NULL, 
 (SELECT PK_User FROM [User] WHERE email = 'enrique.bermudez@ucr.ac.cr'), 'Empleado');

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny) VALUES 
(304560123, 'Supervisor', 'Tiempo completo', 600000, '2023-12-01', 'Administración', 3102234567);

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
