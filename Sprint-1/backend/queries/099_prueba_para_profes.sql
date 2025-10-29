USE Fiesta_Fries_DB;
GO

-- ============================================
-- EMPRESA PI MENSUAL
-- ============================================

INSERT INTO [User](email, [password], active, [admin])
VALUES('pi.empleador@gmail.com', '123456', 1, 0); 
GO

DECLARE @userGuid UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid = PK_User 
FROM [User] 
WHERE email = 'pi.empleador@gmail.com';

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
    550010001,
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
    [CedulaJuridica],
    [Nombre],
    [DueñoEmpresa],
    [Telefono],
    [DireccionEspecifica],
    [NoMaxBeneficios],
    [DiaPago],
    [FrecuenciaPago],
    [FechaCreacion]
)
VALUES(
    550020002,
    'Empresa PI',
    550010001,                 
    88887777,                 
    'San José, Pavas, 400m sur de la iglesia',
    3,                         
    30,                        
    'Mensual',                
    '2025-09-30'            
);
GO

UPDATE Empresa
SET [FechaCreacion] = '2025-01-01'
WHERE [CedulaJuridica] = 550020002;

-- ============================================
-- Caso Número 2: Pedro Vargas
-- ============================================

INSERT INTO [User](email, [password], active, [admin])
VALUES('pedro.vargas@gmail.com', '123456', 1, 0);
GO

DECLARE @userGuid_Empleado UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid_Empleado = PK_User 
FROM [User]
WHERE email = 'pedro.vargas@gmail.com';

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
    152700726,
    'Pedro',
    'Vargas',
    '2000-01-16',
    'San José, Costa Rica',
    88889999,
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
    152700726,          
    'Analista',         
    'Tiempo completo',  
    500000,             
    '2025-01-02',       
    'Operaciones',      
    550020002         
);
GO

-- ============================================
-- Caso número 3: Ana Salas
-- ============================================

INSERT INTO [User](email, [password], active, [admin])
VALUES('ana.salas@gmail.com', '123456', 1, 0);
GO

DECLARE @userGuid_Empleado UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid_Empleado = PK_User 
FROM [User]
WHERE email = 'ana.salas@gmail.com';

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
    208760987,
    'Ana',
    'Salas',
    '1992-12-23',
    'San José, Costa Rica',
    88889998,
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
    208760987,
    'Analista',
    'Tiempo completo',
    1000000,
    '2025-01-02',
    'Operaciones',
    550020002
);
GO

-- ============================================
-- Caso número 4: Juan Solano
-- ============================================

INSERT INTO [User](email, [password], active, [admin])
VALUES('juan.solano@gmail.com', '123456', 1, 0);
GO

DECLARE @userGuid_Empleado UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid_Empleado = PK_User 
FROM [User]
WHERE email = 'juan.solano@gmail.com';

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
    309870267,
    'Juan',
    'Solano',
    '1985-11-17',
    'San José, Costa Rica',
    88889997,
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
    309870267,
    'Analista',
    'Tiempo completo',
    4000000,
    '2025-01-02',
    'Operaciones',
    550020002
);
GO

-- ============================================
-- Caso número 5: Beneficio Gimnasio
-- ============================================

INSERT INTO Beneficio(
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550020002,        
    'Gimnasio',
    'Monto Fijo',
    'Empresa',
    35000,
    'Beneficio'
);
GO

-- ============================================
-- Caso número 6: Beneficio Educación
-- ============================================

INSERT INTO Beneficio(
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550020002,        
    'Educación',
    'Porcentual',
    'Empresa',
    3.00,
    'Beneficio'
);
GO

-- ============================================
-- Caso número 7: Beneficio Seguro privado
-- ============================================

INSERT INTO Beneficio(
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550020002,        
    'Seguro privado',
    'API',
    'Empresa',
    NULL,
    'Deducción'
);
GO

-- ============================================
-- Caso número 8: Pensión voluntaria
-- ============================================

INSERT INTO Beneficio(
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550020002,
    'Pensión voluntaria de vida',
    'API',
    'Empleado',
    NULL,
    'Deducción'
);
GO

-- ============================================
-- Caso número 9: Asociación Solidarista
-- ============================================

INSERT INTO Beneficio(
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550020002,
    'Asociación Solidarista',
    'API',
    'Empleado',
    NULL,
    'Deducción'
);
GO

-- ============================================
-- Caso número 10: Asignar Gimnasio a Pedro
-- ============================================

DECLARE @BenefitId INT;
DECLARE @BenefitType VARCHAR(50);

SELECT TOP 1 
    @BenefitId = IdBeneficio,
    @BenefitType = Tipo
FROM Beneficio 
WHERE Nombre = 'Gimnasio' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(152700726, @BenefitId) = 1
BEGIN
    INSERT INTO EmployeeBenefit(
        employeeId,
        benefitId,
        benefitType
    )
    VALUES(
        152700726,
        @BenefitId,
        @BenefitType
    );
END
GO

-- ============================================
-- Caso número 11: Asignar Educación a Ana
-- ============================================

DECLARE @BenefitId INT;
DECLARE @BenefitType VARCHAR(50);

SELECT TOP 1 
    @BenefitId = IdBeneficio,
    @BenefitType = Tipo
FROM Beneficio 
WHERE Nombre = 'Educación' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(208760987, @BenefitId) = 1
BEGIN
    INSERT INTO EmployeeBenefit(
        employeeId,
        benefitId,
        benefitType
    )
    VALUES(
        208760987, 
        @BenefitId,
        @BenefitType
    );
END
GO

-- ============================================
-- Caso número 12: Seguro privado a Juan
-- ============================================

DECLARE @BenefitId INT;
DECLARE @BenefitType VARCHAR(50);

SELECT TOP 1 
    @BenefitId = IdBeneficio,
    @BenefitType = Tipo
FROM Beneficio 
WHERE Nombre = 'Seguro privado' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(309870267, @BenefitId) = 1
BEGIN
    INSERT INTO EmployeeBenefit(
        employeeId,
        benefitId,
        dependentsCount,
        benefitType
    )
    VALUES(
        309870267,  
        @BenefitId,
        2,
        @BenefitType
    );
END
GO

-- ============================================
-- Caso Número 13: Pensión voluntaria a Juan
-- ============================================

DECLARE @BenefitId INT;
DECLARE @BenefitType VARCHAR(50);

SELECT TOP 1 
    @BenefitId = IdBeneficio,
    @BenefitType = Tipo
FROM Beneficio 
WHERE Nombre = 'Pensión voluntaria de vida' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(309870267, @BenefitId) = 1
BEGIN
    INSERT INTO EmployeeBenefit(
        employeeId,
        benefitId,
        pensionType,
        benefitType
    )
    VALUES(
        309870267,  
        @BenefitId,
        'B',
        @BenefitType
    );
END
GO

-- ============================================
-- Caso 14: Asociación Solidarista a Juan
-- ============================================

DECLARE @BenefitId INT;
DECLARE @BenefitType VARCHAR(50);

SELECT TOP 1 
    @BenefitId = IdBeneficio,
    @BenefitType = Tipo
FROM Beneficio 
WHERE Nombre = 'Asociación Solidarista' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(309870267, @BenefitId) = 1
BEGIN
    INSERT INTO EmployeeBenefit(
        employeeId,
        benefitId,
        benefitType
    )
    VALUES(
        309870267,  
        @BenefitId,
        @BenefitType
    );
END
GO

-- ============================================
-- caso 15: Registrar horas de octubre
-- ============================================

DECLARE @StartDate DATE = '2025-10-06';
DECLARE @EndDate DATE = '2025-11-03';
DECLARE @CurrentWeekStart DATE;
DECLARE @EmployeeId INT;
DECLARE @CurrentDay DATE;

DECLARE EmployeeCursor CURSOR FOR
SELECT id FROM Empleado WHERE idCompny = 550020002;

OPEN EmployeeCursor;
FETCH NEXT FROM EmployeeCursor INTO @EmployeeId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @CurrentWeekStart = @StartDate;

    WHILE @CurrentWeekStart < @EndDate
    BEGIN
        EXEC sp_GetOrCreateWeek @CurrentWeekStart, @EmployeeId;

        DECLARE @i INT = 0;
        WHILE @i < 5
        BEGIN
            SET @CurrentDay = DATEADD(DAY, @i, @CurrentWeekStart);

            EXEC sp_GetOrCreateDay @CurrentDay, @CurrentWeekStart, @EmployeeId;
            EXEC sp_AddHoursToDay 9, @CurrentDay, @CurrentWeekStart, @EmployeeId;

            SET @i = @i + 1;
        END

        SET @CurrentWeekStart = DATEADD(WEEK, 1, @CurrentWeekStart);
    END

    FETCH NEXT FROM EmployeeCursor INTO @EmployeeId;
END

CLOSE EmployeeCursor;
DEALLOCATE EmployeeCursor;
GO

-- ============================================
-- EMPRESA PI QUINCENAL/SEMANAL
-- ============================================

INSERT INTO [User](email, [password], active, [admin])
VALUES('pi.empleador@gmail2.com', '123456', 1, 0); 
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

-- ============================================
-- Caso número 2: Mariana Vásquez
-- ============================================

INSERT INTO [User](email, [password], active, [admin])
VALUES('mariana.vasquez@gmail.com', '123456', 1, 0);
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

-- ============================================
-- Caso Número 3: Juan Vásquez
-- ============================================

INSERT INTO [User](email, [password], active, [admin])
VALUES('juan.vasquez@gmail.com', '123456', 1, 0);
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

-- ============================================
-- caso número 4: Beneficio Gimnasio empresa 2
-- ============================================

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

-- ============================================
-- caso número 5: Beneficio Educación empresa 2
-- ============================================

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

-- ============================================
-- Caso número 6: Asignar Gimnasio a Juan V.
-- ============================================

DECLARE @BenefitId INT;
DECLARE @BenefitType VARCHAR(50);

SELECT TOP 1 
    @BenefitId = IdBeneficio,
    @BenefitType = Tipo
FROM Beneficio 
WHERE Nombre = 'Gimnasio' AND CedulaJuridica = 550030003;

INSERT INTO EmployeeBenefit(
    employeeId, 
    benefitId,
    benefitType
)
VALUES(
    208760988, 
    @BenefitId,
    @BenefitType
);
GO

-- ============================================
-- caso número 7: Asignar Educación a Juan V.
-- ============================================

DECLARE @BenefitId INT;
DECLARE @BenefitType VARCHAR(50);

SELECT TOP 1 
    @BenefitId = IdBeneficio,
    @BenefitType = Tipo
FROM Beneficio 
WHERE Nombre = 'Educación' AND CedulaJuridica = 550030003;

INSERT INTO EmployeeBenefit(
    employeeId, 
    benefitId,
    benefitType
)
VALUES(
    208760988, 
    @BenefitId,
    @BenefitType
);
GO

-- ============================================
-- Caso número 8: Registrar horas octubre empresa 2
-- ============================================

DECLARE @WeekStart DATE;
DECLARE @EmployeeId INT;
DECLARE EmployeeCursor CURSOR FOR
SELECT id FROM Empleado WHERE idCompny = 550030003;

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

    SET @WeekStart = '2025-10-16';
    EXEC sp_GetOrCreateWeek @WeekStart, @EmployeeId;
    SET @i = 0;
    WHILE @i < 5
    BEGIN
        SET @CurrentDay = DATEADD(DAY, @i, @WeekStart);
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