USE Fiesta_Fries_DB;

BEGIN TRY
    SET NOCOUNT ON;
    BEGIN TRAN;
    DECLARE @InsertedUsers TABLE (PK_User UNIQUEIDENTIFIER);

    INSERT INTO [User] (email, [password], active)
    OUTPUT INSERTED.PK_User INTO @InsertedUsers
    VALUES ('juanito@gmail.com', '123456', 1);

    DECLARE @UserId UNIQUEIDENTIFIER;
    SELECT TOP (1) @UserId = PK_User FROM @InsertedUsers;

    INSERT INTO [Persona] (
        id,
        firstName,
        secondName,
        birthdate,
        direction,
        personalPhone,
        homePhone,
        uniqueUser,
        personType
    )
    VALUES (
        119180744,
        'Juanito',
        'Romero',
        '2000-11-11',   
        'Palo de mango',
        88772576,
        88772526,
        @UserId,
        'empleador'
    );

    DECLARE @cedulaempleador INT;
    SELECT @cedulaempleador = id
    FROM Persona
    WHERE uniqueUser = @UserId;

    INSERT INTO Empresa (
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
    VALUES (
        1111111112,
        'Kurama',
        @cedulaempleador,
        88772578,
        'Palo de mango',
        5,
        30,
        'Mensual',
        GETDATE()
    );

  
    DELETE FROM @InsertedUsers;

   
    INSERT INTO [User] (email, [password], active)
    OUTPUT INSERTED.PK_User INTO @InsertedUsers
    VALUES ('ddoodle444@gmail.com', 'Ignacio10@', 1);

    DECLARE @UserId2 UNIQUEIDENTIFIER;
    SELECT TOP (1) @UserId2 = PK_User FROM @InsertedUsers;

    INSERT INTO [Persona] (
        id,
        firstName,
        secondName,
        birthdate,
        direction,
        personalPhone,
        homePhone,
        uniqueUser,
        personType
    )
    VALUES (
        119180749,
        'Naruto',
        'Uzumaki',
        '2004-10-10',
        'Palo de mango',
        88772564,
        22289008,
        @UserId2,
        'Empleado'
    );

    INSERT INTO Empleado (
        id,
        position,
        employmentType,
        salary,
        hireDate,
        department,
        idCompny
    )
    VALUES (
        119180749,
        'Shinobi',
        'Tiempo Completo',
        100000000,
        '2024-01-01',
        'Konoha',
        1111111112
    );

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRAN;

    THROW;
END CATCH;
