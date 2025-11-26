USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO


USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

INSERT INTO [Fiesta_Fries_DB].[User](email, [password])
VALUES('pi.empleador@gmail.com', '123456'); 
GO


DECLARE @userGuid UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid = PK_User 
FROM [User] 
WHERE email = 'pi.empleador@gmail.com';


INSERT INTO [Fiesta_Fries_DB].[Persona](
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

INSERT INTO [Fiesta_Fries_DB].[Persona](
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

UPDATE [Fiesta_Fries_DB].[Persona] SET [FechaCreacion] = '2025-01-01'
WHERE [CedulaJuridica] = 550020002;


--Caso Número 2

INSERT INTO [Fiesta_Fries_DB].[User](email, [password])
VALUES('pedro.vargas@gmail.com', '123456');
GO

DECLARE @userGuid_Empleado UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid_Empleado = PK_User 
FROM [User]
WHERE email = 'pedro.vargas@gmail.com';


INSERT INTO [Fiesta_Fries_DB].[Persona](
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

INSERT INTO [Fiesta_Fries_DB].[Persona](
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

-- Caso número 3
INSERT INTO [Fiesta_Fries_DB].[User](email, [password])
VALUES('ana.salas@gmail.com', '123456');
GO

DECLARE @userGuid_Empleado UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid_Empleado = PK_User 
FROM [User]
WHERE email = 'ana.salas@gmail.com';

INSERT INTO [Fiesta_Fries_DB].[Persona](
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

INSERT INTO [Fiesta_Fries_DB].[Persona](
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


--Caso número 4
INSERT INTO [Fiesta_Fries_DB].[User](email, [password])
VALUES('juan.solano@gmail.com', '123456');
GO

DECLARE @userGuid_Empleado UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid_Empleado = PK_User 
FROM [User]
WHERE email = 'juan.solano@gmail.com';

INSERT INTO [Fiesta_Fries_DB].[Persona](
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

INSERT INTO [Fiesta_Fries_DB].[Persona](
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
--Caso número 5
INSERT INTO [Fiesta_Fries_DB].[Persona](
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

--Caso número 6

INSERT INTO [Fiesta_Fries_DB].[Persona](
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

--Caso número 7
INSERT INTO [Fiesta_Fries_DB].[Persona](
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
    'Beneficio'
);
GO

--Caso número 8
INSERT INTO [Fiesta_Fries_DB].[Persona](
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550020002,        -- Empresa PI
    'Pensión voluntaria de vida',
    'API',
    'Empleado',
    NULL,
    'Beneficio'
);
GO

--Caso número 9
INSERT INTO [Fiesta_Fries_DB].[Persona](
    CedulaJuridica,
    Nombre,
    Tipo,
    QuienAsume,
    Valor,
    Etiqueta
)
VALUES(
    550020002,        -- Empresa PI
    'Asociación Solidarista',
    'API',
    'Empleado',
    NULL,
    'Beneficio'
);
GO

--Caso núermo 10

DECLARE @BenefitId INT;
SELECT TOP 1 @BenefitId = IdBeneficio 
FROM Beneficio 
WHERE Nombre = 'Gimnasio' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(152700726, @BenefitId) = 1
BEGIN
    INSERT INTO [Fiesta_Fries_DB].[Persona](
        employeeId,
        benefitId
    )
    VALUES(
        152700726,
        @BenefitId
    );
END
GO

--Caso número 11

DECLARE @BenefitId INT;
SELECT TOP 1 @BenefitId = IdBeneficio 
FROM Beneficio 
WHERE Nombre = 'Educación' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(208760987, @BenefitId) = 1
BEGIN
    INSERT INTO [Fiesta_Fries_DB].[Persona](
        employeeId,
        benefitId
    )
    VALUES(
        208760987, 
        @BenefitId
    );
END
GO

USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

--Caso número 12
DECLARE @BenefitId INT;
SELECT TOP 1 @BenefitId = IdBeneficio 
FROM Beneficio 
WHERE Nombre = 'Seguro privado' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(309870267, @BenefitId) = 1
BEGIN
    INSERT INTO [Fiesta_Fries_DB].[Persona](
        employeeId,
        benefitId,
        dependentsCount
    )
    VALUES(
        309870267,  
        @BenefitId,
        2          
    );
END
GO

--Caso Número 13
DECLARE @BenefitId INT;
SELECT TOP 1 @BenefitId = IdBeneficio 
FROM Beneficio 
WHERE Nombre = 'Pensión voluntaria de vida' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(309870267, @BenefitId) = 1
BEGIN
    INSERT INTO [Fiesta_Fries_DB].[Persona](
        employeeId,
        benefitId,
        pensionType
    )
    VALUES(
        309870267,  
        @BenefitId,
        'B'         
    );
END
GO


--Caso 14 

DECLARE @BenefitId INT;
SELECT TOP 1 @BenefitId = IdBeneficio 
FROM Beneficio 
WHERE Nombre = 'Asociación Solidarista' AND CedulaJuridica = 550020002;

IF dbo.CanEmployeeSelectBenefit(309870267, @BenefitId) = 1
BEGIN
    INSERT INTO [Fiesta_Fries_DB].[Persona](
        employeeId,
        benefitId
    )
    VALUES(
        309870267,  
        @BenefitId
    );
END
GO

--caso 15
DECLARE @StartDate DATE = '2025-10-06';  -- Primer lunes de octubre 2025
DECLARE @EndDate DATE = '2025-11-03';    -- Primer lunes de noviembre 2025
DECLARE @CurrentWeekStart DATE;
DECLARE @EmployeeId INT;
DECLARE @CurrentDay DATE;

DECLARE EmployeeCursor CURSOR FOR
SELECT id FROM [Fiesta_Fries_DB].[Persona];

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






