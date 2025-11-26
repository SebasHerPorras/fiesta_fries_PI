
USE Fiesta_Fries_DB;
GO

INSERT INTO [User](email, [password], active, [admin])
VALUES('selenium_test@gmail.com', '123456', 1, 0); 
GO

DECLARE @userGuid UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid = PK_User 
FROM [User] 
WHERE email = 'selenium_test@gmail.com';

INSERT INTO Persona (id, firstName, secondName, birthDate,direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (999999999, 'Selenium', 'test', '1985-11-17', 'San José, Costa Rica', '8888-1111', NULL, @userGuid, 'Empleador', 0);


INSERT INTO Empresa (CedulaJuridica, Nombre, DueñoEmpresa, Telefono, DireccionEspecifica, NoMaxBeneficios, DiaPago, FrecuenciaPago, FechaCreacion)
VALUES (9999999999, 'Empresa SeleniumTest', 999999999, 22243333, 'San José, Centro Empresarial', 3, 1, 'Mensual', '2025-01-01');

