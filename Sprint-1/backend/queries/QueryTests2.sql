USE Fiesta_Fries_DB;
GO

--Caso Número 1

INSERT INTO [User](email, [password])
VALUES('pi.empleador@gmail2.com', '123456'); 
GO

DECLARE @userGuid UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid = PK_User 
FROM [User] 
WHERE email = 'pi.empleador@gmail2.com';

INSERT INTO Persona(
    id,
    firstName,
    secondName,
    birthdate,
    direction,
    personalPhone,
    uniqueUser,
    personType
)
VALUES(
    550010002,
    'Pablo',
    'Ibarra',
    '1990-05-12',
    'San José, Costa Rica',
    88887777,
    @userGuid,
    'Empleador'
);
GO

INSERT INTO Empresa(
    CedulaJuridica,
    Nombre,
    DueñoEmpresa,
    Telefono,
    DireccionEspecifica,
    NoMaxBeneficios,
    DiaPago,
    FrecuenciaPago,
    FechaCreacion
)
VALUES(
    550030003,
    'Empresa PI Quincenal',
    550010002,       
    88880000,
    'San José, Costa Rica',
    2,
    7,               
    'Semanal',
    '2024-12-01'
);
GO

--Caso número 2
INSERT INTO [User](email, [password])
VALUES('mariana.vasquez@gmail.com', '123456');
GO

DECLARE @userGuid_Empleado UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid_Empleado = PK_User 
FROM [User] 
WHERE email = 'mariana.vasquez@gmail.com';

INSERT INTO Persona(
    id,
    firstName,
    secondName,
    birthdate,
    direction,
    personalPhone,
    uniqueUser,
    personType
)
VALUES(
    402830876,
    'Mariana',
    'Vásquez',
    '1999-01-28',
    'San José, Costa Rica',
    88880123,
    @userGuid_Empleado,
    'Empleado'
);
GO

INSERT INTO Empleado(
    id,
    position,
    employmentType,
    salary,
    hireDate,
    department,
    idCompny
)
VALUES(
    402830876,
    'Profesional',
    'Por horas',
    15000,
    '2025-02-02',
    'Servicios',
    550030003
);
GO


--Caso Número 3

INSERT INTO [User](email, [password])
VALUES('juan.vasquez@gmail.com', '123456');
GO

DECLARE @userGuid_Empleado UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid_Empleado = PK_User 
FROM [User] 
WHERE email = 'juan.vasquez@gmail.com';

INSERT INTO Persona(
    id,
    firstName,
    secondName,
    birthdate,
    direction,
    personalPhone,
    uniqueUser,
    personType
)
VALUES(
    208760988,
    'Juan',
    'Vásquez',
    '1991-12-23',
    'San José, Costa Rica',
    88880234,
    @userGuid_Empleado,
    'Empleado'
);
GO

INSERT INTO Empleado(
    id,
    position,
    employmentType,
    salary,
    hireDate,
    department,
    idCompny
)
VALUES(
    208760988,
    'Analista',
    'Tiempo completo',
    1500000,
    '2025-10-02',
    'Operaciones',
    550030003
);
GO

--caso número 4
INSERT INTO Beneficio(
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550030003,
    'Gimnasio',
    'Monto Fijo',
    'Empresa',
    35000,
    'Beneficio'
);
GO

--caso número 5
INSERT INTO Beneficio(
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550030003,
    'Educación',
    'Porcentual',
    'Empresa',
    3.00,
    'Beneficio'
);
GO

--Caso núermo 6
DECLARE @BenefitId INT;
SELECT TOP 1 @BenefitId = IdBeneficio 
FROM Beneficio 
WHERE Nombre = 'Gimnasio' AND CedulaJuridica = 550030003;

    INSERT INTO EmployeeBenefit(employeeId, benefitId)
    VALUES(208760988, @BenefitId);
GO

--caso número7
DECLARE @BenefitId INT;
SELECT TOP 1 @BenefitId = IdBeneficio 
FROM Beneficio 
WHERE Nombre = 'Educación' AND CedulaJuridica = 550030003;

    INSERT INTO EmployeeBenefit(employeeId, benefitId)
    VALUES(208760988, @BenefitId);
GO

--Csao número8
DECLARE @WeekStart DATE;
DECLARE @EmployeeId INT;
DECLARE EmployeeCursor CURSOR FOR
SELECT id FROM Empleado;

OPEN EmployeeCursor;
FETCH NEXT FROM EmployeeCursor INTO @EmployeeId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @WeekStart = '2025-10-01';
    EXEC sp_GetOrCreateWeek @WeekStart, @EmployeeId;
    DECLARE @i INT = 0;
    WHILE @i < 5
    BEGIN
        DECLARE @CurrentDay DATE = DATEADD(DAY, @i, @WeekStart);
        EXEC sp_GetOrCreateDay @CurrentDay, @WeekStart, @EmployeeId;
        EXEC sp_AddHoursToDay 9, @CurrentDay, @WeekStart, @EmployeeId;
        SET @i = @i + 1;
    END

    SET @WeekStart = '2025-10-08';
    EXEC sp_GetOrCreateWeek @WeekStart, @EmployeeId;
    SET @i = 0;
    WHILE @i < 5
    BEGIN
        SET @CurrentDay = DATEADD(DAY, @i, @WeekStart);
        EXEC sp_GetOrCreateDay @CurrentDay, @WeekStart, @EmployeeId;
        EXEC sp_AddHoursToDay 9, @CurrentDay, @WeekStart, @EmployeeId;
        SET @i = @i + 1;
    END

    FETCH NEXT FROM EmployeeCursor INTO @EmployeeId;
END

CLOSE EmployeeCursor;
DEALLOCATE EmployeeCursor;
GO

--caso número 10

DECLARE @WeekStart DATE;
DECLARE @EmployeeId INT;
DECLARE EmployeeCursor CURSOR FOR
SELECT id FROM Empleado;

OPEN EmployeeCursor;
FETCH NEXT FROM EmployeeCursor INTO @EmployeeId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @WeekStart = '2025-10-16';
    EXEC sp_GetOrCreateWeek @WeekStart, @EmployeeId;
    DECLARE @i INT = 0;
    WHILE @i < 5
    BEGIN
        DECLARE @CurrentDay DATE = DATEADD(DAY, @i, @WeekStart);
        EXEC sp_GetOrCreateDay @CurrentDay, @WeekStart, @EmployeeId;
        EXEC sp_AddHoursToDay 9, @CurrentDay, @WeekStart, @EmployeeId;
        SET @i = @i + 1;
    END

    SET @WeekStart = '2025-10-22';
    EXEC sp_GetOrCreateWeek @WeekStart, @EmployeeId;
    SET @i = 0;
    WHILE @i < 5
    BEGIN
        SET @CurrentDay = DATEADD(DAY, @i, @WeekStart);
        EXEC sp_GetOrCreateDay @CurrentDay, @WeekStart, @EmployeeId;
        EXEC sp_AddHoursToDay 9, @CurrentDay, @WeekStart, @EmployeeId;
        SET @i = @i + 1;
    END

    FETCH NEXT FROM EmployeeCursor INTO @EmployeeId;
END

CLOSE EmployeeCursor;
DEALLOCATE EmployeeCursor;
GO
