-- =============================================
-- SCRIPT DE CASOS DE PRUEBA - SPRINT 3
-- Empresa: Empresa Sprint 3 y Empresa Integradora
-- Fecha: Noviembre 2025
-- =============================================
USE Fiesta_Fries_DB;
GO

PRINT '========================================';
PRINT 'INICIANDO SCRIPT DE CASOS DE PRUEBA';
PRINT '========================================';
GO

-- =============================================
-- PASO 1: Crear empleador Ana Madrigal
-- Nacida: 17/Nov/1985
-- =============================================
PRINT '';
PRINT 'PASO 1: Creando empleador Ana Madrigal...';

INSERT INTO [User] (email, [password], active, [admin])
VALUES ('ana.madrigal@empresa.com', '123456', 1, 0);

DECLARE @UserAnaGuid UNIQUEIDENTIFIER = (SELECT PK_User FROM [User] WHERE email = 'ana.madrigal@empresa.com');

INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (117851985, 'Ana', 'Madrigal', '1985-11-17', 'San José, Costa Rica', '8888-1111', NULL, @UserAnaGuid, 'Empleador', 0);

PRINT '✓ Ana Madrigal creada como empleador (Cédula: 117851985)';
GO

-- =============================================
-- PASO 2: Crear Empresa Sprint 3
-- Cédula: 3-101-123456
-- Máximo beneficios: 3
-- Frecuencia pago: Mensual
-- =============================================
PRINT '';
PRINT 'PASO 2: Creando Empresa Sprint 3...';

INSERT INTO Empresa (CedulaJuridica, Nombre, DueñoEmpresa, Telefono, DireccionEspecifica, NoMaxBeneficios, DiaPago, FrecuenciaPago, FechaCreacion)
VALUES (3108123456, 'Empresa Sprint 3', 117851985, 22223333, 'San José, Centro Empresarial', 3, 1, 'Mensual', '2025-01-01');

PRINT '✓ Empresa Sprint 3 creada (Cédula: 3108123456, Pago: Mensual día 1)';
GO

-- =============================================
-- PASOS 3-7: Crear beneficios para Empresa Sprint 3
-- =============================================
PRINT '';
PRINT 'PASOS 3-7: Creando beneficios para Empresa Sprint 3...';

-- PASO 3: Beneficio Gimnasio (Monto fijo ₡25,000)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3108123456, 'Gimnasio', 'Monto Fijo', 'Empresa', 25000.00, 'Beneficio', 0);
PRINT '✓ Beneficio Gimnasio creado (Monto fijo: ₡25,000)';

-- PASO 4: Beneficio Educación (Porcentual 3%)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3108123456, 'Educación', 'Porcentual', 'Empresa', 3.00, 'Beneficio', 0);
PRINT '✓ Beneficio Educación creado (Porcentual: 3%)';

-- PASO 5: Beneficio Seguro Privado (API)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3108123456, 'Seguro Privado', 'API', 'Empresa', NULL, 'Deducción', 0);
PRINT '✓ Beneficio Seguro Privado creado (API: SeguroPrivado)';

-- PASO 6: Beneficio Pensión Voluntaria (API)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3108123456, 'Pension Voluntaria', 'API', 'Empleado', NULL, 'Deducción', 0);
PRINT '✓ Beneficio Pensión Voluntaria de Vida creado (API: PensionesVoluntarias)';

-- PASO 7: Beneficio Asociación Solidarista (API)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3108123456, 'Asociación Solidarista', 'API', 'Empleado', NULL, 'Beneficio', 0);
PRINT '✓ Beneficio Asociación Solidarista creado (API: AsociacionSolidarista)';
GO

-- =============================================
-- PASO 8: Crear empleada Ana Madrigal COMO EMPLEADA (DIFERENTE PERSONA)
-- Salario: ₡4,845,000
-- Fecha inicio: 1/May/2025
-- Puesto: Gerente
-- NOTA: Esta es una Ana Madrigal DIFERENTE a la dueña
-- =============================================
PRINT '';
PRINT 'PASO 8: Creando empleada Ana Madrigal (diferente a la dueña)...';

INSERT INTO [User] (email, [password], active, [admin])
VALUES ('ana.madrigal.empleada@empresa.com', '123456', 1, 0);

DECLARE @UserAnaEmpleadaGuid UNIQUEIDENTIFIER = (SELECT PK_User FROM [User] WHERE email = 'ana.madrigal.empleada@empresa.com');

INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (117111985, 'Ana', 'Madrigal', '1985-11-17', 'San José, Sabana', '8888-7777', NULL, @UserAnaEmpleadaGuid, 'Empleado', 0);

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny, IsDeleted)
VALUES (117111985, 'Gerente', 'Tiempo Completo', 4845000, '2025-05-01', 'Gerencia', 3108123456, 0);

PRINT '✓ Ana Madrigal (empleada) creada (Gerente, ₡4,845,000, Cédula: 117111985)';
GO


-- =============================================
-- INSERCIÓN DE HORAS SEMANALES - ANA MADRIGAL (117111985)
-- PERIODO: MAYO 2025 (1 al 31 de mayo)
-- Contratada: 1/Mayo/2025
-- =============================================
PRINT '';
PRINT 'Insertando horas semanales para Ana Madrigal - Mayo 2025...';

-- MAYO 2025: Semanas del mes (del 1 al 31 de mayo)
-- Semana del 28/04/2025 (lunes) - incluye 1 y 2 de mayo (jueves y viernes)
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-04-28', 117111985, 16);

-- Semana del 05/05/2025 (lunes) - semana completa
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-05-05', 117111985, 40);

-- Semana del 12/05/2025 (lunes) - semana completa
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-05-12', 117111985, 40);

-- Semana del 19/05/2025 (lunes) - semana completa
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-05-19', 117111985, 40);

-- Semana del 26/05/2025 (lunes) - incluye hasta 31 de mayo (sábado) = 5 días
INSERT INTO Semana (start_date, id_employee, hours_count) VALUES ('2025-05-26', 117111985, 40);

PRINT '✓ Horas de Mayo 2025 insertadas para Ana Madrigal (176 horas total)';
GO

-- NOTE: PASO 9 - Ejecutar planilla Mayo 2025 se debe hacer manualmente desde la aplicación

-- =============================================
-- PASO 10: Crear empleada Sara Rodriguez
-- Nacida: 16/Ene/2000
-- Salario: ₡900,000
-- Fecha inicio: 1/Jun/2025
-- Puesto: Desarrollador Jr.
-- =============================================
PRINT '';
PRINT 'PASO 10: Creando empleada Sara Rodriguez...';

INSERT INTO [User] (email, [password], active, [admin])
VALUES ('sara.rodriguez@empresa.com', '123456', 1, 0);

DECLARE @UserSaraGuid UNIQUEIDENTIFIER = (SELECT PK_User FROM [User] WHERE email = 'sara.rodriguez@empresa.com');

INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (116012000, 'Sara', 'Rodriguez', '2000-01-16', 'Heredia, Costa Rica', '8888-2222', NULL, @UserSaraGuid, 'Empleado', 0);

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny, IsDeleted)
VALUES (116012000, 'Desarrollador Jr.', 'Tiempo Completo', 900000, '2025-06-01', 'Desarrollo', 3108123456, 0);

PRINT '✓ Sara Rodriguez creada (Desarrollador Jr., ₡900,000)';
GO

-- =============================================
-- PASO 11: Crear empleada Jenny Durango
-- Nacida: 23/Dic/1988
-- Salario: ₡2,500,000
-- Fecha inicio: 16/Jun/2025
-- Puesto: Desarrollador Senior
-- =============================================
PRINT '';
PRINT 'PASO 11: Creando empleada Jenny Durango...';

INSERT INTO [User] (email, [password], active, [admin])
VALUES ('jenny.durango@empresa.com', '123456', 1, 0);

DECLARE @UserJennyGuid UNIQUEIDENTIFIER = (SELECT PK_User FROM [User] WHERE email = 'jenny.durango@empresa.com');

INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (123121988, 'Jenny', 'Durango', '1988-12-23', 'Cartago, Costa Rica', '8888-3333', NULL, @UserJennyGuid, 'Empleado', 0);

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny, IsDeleted)
VALUES (123121988, 'Desarrollador Senior', 'Tiempo Completo', 2500000, '2025-06-16', 'Desarrollo', 3108123456, 0);

PRINT '✓ Jenny Durango creada (Desarrollador Senior, ₡2,500,000)';
GO

-- =============================================
-- PASOS 12 y 13: Asignar beneficios a empleados para planilla Junio 2025

DECLARE @benefitID1 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Asociación Solidarista');

INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (123121988, @benefitID1, NULL, NULL, 'Asociación Solidarista', NULL, 'API', 0);


DECLARE @benefitID2 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Pension Voluntaria');

INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (117111985, @benefitID2, 'C', NULL, 'Pension Voluntaria', NULL, 'API', 0);


-- AGREGAR HORAS PARA LOS EMPLEADOS PARA EL MES DE JUNIO 2025 PARA QUE SE TOME EN CUENTA PARA EL PAGO DE JUNIO 2025
/*
INSERT INTO EmployeeHours (employeeId, month, year, hoursWorked, IsDeleted)
VALUES (117111985, 6, 2025, 160, 0);
*/

--  =============================================
-- PASO 14:
-- GENERERAR PLANILLAS JUNIO 2025
--  =============================================


-- =============================================
-- PASOS 15-16: Asignar beneficios adicionales a empleados para planilla Julio

DECLARE @benefitID3 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Gimnasio');

INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (116012000, @benefitID3, NULL, NULL, 'Gimnasio', 25000, 'Monto Fijo', 0);


DECLARE @benefitID4 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Seguro Privado');

INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (123121988, @benefitID4, NULL, 3, 'Seguro Privado', NULL, 'API', 0);


-- AGREGAR HORAS PARA LOS EMPLEADOS PARA EL MES  JULIO , AGOSTO , SEPTIEMBRE 2025 PARA QUE SE TOME EN CUENTA PARA EL PAGO DE ESOS MESES
/*
INSERT INTO EmployeeHours (employeeId, month, year, hoursWorked, IsDeleted)
VALUES (117111985, 7, 2025, 160, 0);
*/

-- =============================================
-- PASOS 17-19
-- GENERAR PLANILLAS JULIO 2025 , AGOSTO 2025, SEPTIEMBRE 2025
--  =============================================




















-- =============================================
-- PASO 20: Promoción Sara Rodriguez (1/Oct/2025)
-- Nuevo puesto: Desarrollador Mid
-- Nuevo salario: ₡1,600,000
-- =============================================
PRINT '';
PRINT 'PASO 20: Aplicando promoción de Sara Rodriguez...';

UPDATE Empleado
SET position = 'Desarrollador Mid',
    salary = 1600000
WHERE id = 116012000;

PRINT '✓ Sara Rodriguez promovida (Desarrollador Mid, ₡1,600,000)';
GO

-- =============================================
-- PASO 21: Crear empleado John Smith
-- Nacido: 25/Mar/1995
-- Salario: ₡1,800,000
-- Puesto: Product Owner
-- Fecha inicio: 1/Oct/2025
-- =============================================
PRINT '';
PRINT 'PASO 21: Creando empleado John Smith...';

INSERT INTO [User] (email, [password], active, [admin])
VALUES ('john.smith@empresa.com', '123456', 1, 0);

DECLARE @UserJohnGuid UNIQUEIDENTIFIER = (SELECT PK_User FROM [User] WHERE email = 'john.smith@empresa.com');

INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (125031995, 'John', 'Smith', '1995-03-25', 'San José, Escazú', '8888-4444', NULL, @UserJohnGuid, 'Empleado', 0);

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny, IsDeleted)
VALUES (125031995, 'Product Owner', 'Tiempo Completo', 1800000, '2025-10-01', 'Producto', 3108123456, 0);

PRINT '✓ John Smith creado (Product Owner, ₡1,800,000)';
GO

-- =============================================
-- PASOS 22-23: Crear beneficios adicionales
-- =============================================
PRINT '';
PRINT 'PASOS 22-23: Creando beneficios adicionales...';

-- PASO 22: Beneficio Spa (Monto fijo ₡35,000)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3108123456, 'Spa', 'Monto Fijo', 'Empresa', 35000.00, 'Beneficio', 0);
PRINT '✓ Beneficio Spa creado (Monto fijo: ₡35,000)';

-- PASO 23: Beneficio Club Pass (Monto fijo ₡40,000)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3108123456, 'Club Pass', 'Monto Fijo', 'Empresa', 40000.00, 'Beneficio', 0);
PRINT '✓ Beneficio Club Pass creado (Monto fijo: ₡40,000)';
GO

-- NOTE: PASO 24-25 - Asignación de beneficios a John Smith y planilla Octubre se hacen desde la aplicación

DECLARE @benefitID5 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Seguro Privado');

INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (125031995, @benefitID5, NULL, 1, 'Seguro Privado', NULL, 'API', 0);

DECLARE @benefitID6 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Club Pass');

INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (125031995, @benefitID6, NULL, NULL, 'Club Pass', 40000, 'Monto Fijo', 0);

DECLARE @benefitID7 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Pension Voluntaria');

INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (125031995, @benefitID7, 'A', NULL, 'Pension Voluntaria', NULL, 'API', 0);


-- ==================
-- PLANILLA DE OCTUBRE 2025
-- ==================


-- =============================================
-- PASO 26: Crear empleada Jane Doe
-- Nacida: 25/Mar/1991
-- Salario: ₡1,600,000
-- Fecha inicio: 1/Nov/2025
-- =============================================
PRINT '';
PRINT 'PASO 26: Creando empleada Jane Doe...';

INSERT INTO [User] (email, [password], active, [admin])
VALUES ('jane.doe@empresa.com', '123456', 1, 0);

DECLARE @UserJaneGuid UNIQUEIDENTIFIER = (SELECT PK_User FROM [User] WHERE email = 'jane.doe@empresa.com');

INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (125031991, 'Jane', 'Doe', '1991-03-25', 'Alajuela, Costa Rica', '8888-5555', NULL, @UserJaneGuid, 'Empleado', 0);

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny, IsDeleted)
VALUES (125031991, 'Sin Puesto', 'Tiempo Completo', 1600000, '2025-11-01', 'General', 3108123456, 0);

PRINT '✓ Jane Doe creada (Sin Puesto, ₡1,600,000)';
GO



-- NOTE: PASO 27 - Asignación de beneficios a Jane Doe se hace desde la aplicación

DECLARE @benefitID8 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Educación');
INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (125031991, @benefitID8, NULL, NULL, 'Educación', 3.00, 'Porcentual', 0);

DECLARE @benefitID9 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Spa');
INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (125031991, @benefitID9, NULL, NULL, 'Spa', 35000, 'Monto Fijo', 0);

DECLARE @benefitID10 INT = (SELECT IdBeneficio FROM [Beneficio] WHERE Nombre = 'Club Pass');
INSERT INTO EmployeeBenefit (employeeId, benefitId, pensionType, dependentsCount, apiName, benefitValue, benefitType, IsDeleted)
VALUES (125031991, @benefitID10, NULL, NULL, 'Club Pass', 40000, 'Monto Fijo', 0);



-- =============================================
-- PASO 28: Crear Empresa Integradora
-- Cédula: 3-102-654321
-- Máximo beneficios: 2
-- Frecuencia pago: Mensual
-- =============================================
PRINT '';
PRINT 'PASO 28: Creando Empresa Integradora...';

INSERT INTO Empresa (CedulaJuridica, Nombre, DueñoEmpresa, Telefono, DireccionEspecifica, NoMaxBeneficios, DiaPago, FrecuenciaPago, FechaCreacion)
VALUES (3102654321, 'Empresa Integradora', 117851985, 22224444, 'San José, Zona Industrial', 2, 1, 'Mensual', '2025-01-01');

PRINT '✓ Empresa Integradora creada (Cédula: 3102654321, Pago: Mensual día 1)';
GO

-- =============================================
-- PASOS 29-30: Crear beneficios para Empresa Integradora
-- =============================================
PRINT '';
PRINT 'PASOS 29-30: Creando beneficios para Empresa Integradora...';

-- ==============================
-- La empleadore es la misma Ana Madrigal (dueña) de la empresa anterior
-- ==============================


-- PASO 29: Beneficio Asociación Solidarista (API)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3102654321, 'Asociación Solidarista', 'API', 'Compartido', NULL, 'Beneficio', 0);
PRINT '✓ Beneficio Asociación Solidarista creado (API: AsociacionSolidarista)';

-- PASO 30: Beneficio Yoga (Monto fijo ₡15,000)
INSERT INTO Beneficio (CedulaJuridica, Nombre, Tipo, QuienAsume, Valor, Etiqueta, IsDeleted)
VALUES (3102654321, 'Yoga', 'Monto Fijo', 'Empresa', 15000.00, 'Beneficio', 0);
PRINT '✓ Beneficio Yoga creado (Monto fijo: ₡15,000)';
GO

-- =============================================
-- PASO 31: Crear empleado Juan Perez
-- Nacido: 2/Jun/1997
-- Salario: ₡1,200,000
-- Tipo: Servicios Profesionales
-- Fecha inicio: 1/Nov/2024
-- =============================================
PRINT '';
PRINT 'PASO 31: Creando empleado Juan Perez...';

INSERT INTO [User] (email, [password], active, [admin])
VALUES ('juan.perez@empresa.com', '123456', 1, 0);

DECLARE @UserJuanGuid UNIQUEIDENTIFIER = (SELECT PK_User FROM [User] WHERE email = 'juan.perez@empresa.com');

INSERT INTO Persona (id, firstName, secondName, birthdate, direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (102061997, 'Juan', 'Perez', '1997-06-02', 'Puntarenas, Costa Rica', '8888-6666', NULL, @UserJuanGuid, 'Empleado', 0);

INSERT INTO Empleado (id, position, employmentType, salary, hireDate, department, idCompny, IsDeleted)
VALUES (102061997, 'Sin Puesto', 'Servicios Profesionales', 1200000, '2024-11-01', 'Consultoría', 3102654321, 0);

PRINT '✓ Juan Perez creado (Servicios Profesionales, ₡1,200,000)';
GO

-- NOTE: PASOS 32-34 - Asignación de beneficios y planillas Nov 2024 - Oct 2025 se hacen desde la aplicación



PRINT '';
PRINT '========================================';
PRINT 'SCRIPT COMPLETADO EXITOSAMENTE';
PRINT '========================================';
PRINT '';
PRINT 'RESUMEN:';
PRINT '- Empleador creado: 1 (Ana Madrigal)';
PRINT '- Empresas creadas: 2 (Empresa Sprint 3, Empresa Integradora)';
PRINT '- Empleados Empresa Sprint 3: 5';
PRINT '- Empleados Empresa Integradora: 1';
PRINT '- Beneficios Empresa Sprint 3: 7';
PRINT '- Beneficios Empresa Integradora: 2';
PRINT '- Horas trabajadas Mayo 2025: Ana Madrigal (176 horas)';
PRINT '';
PRINT 'NOTA: Las planillas y asignación de beneficios deben';
PRINT 'ejecutarse desde la aplicación web según los pasos indicados.';
PRINT '========================================';
GO


