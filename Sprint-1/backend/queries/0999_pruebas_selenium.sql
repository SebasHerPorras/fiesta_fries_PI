
USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearán bajo el schema Fiesta_Fries_DB
GO

INSERT INTO [Fiesta_Fries_DB].[User](email, [password], active, [admin])
VALUES('selenium_test@gmail.com', '123456', 1, 0); 
GO

DECLARE @userGuid UNIQUEIDENTIFIER;
SELECT TOP 1 @userGuid = PK_User 
FROM [Fiesta_Fries_DB].[User] 
WHERE email = 'selenium_test@gmail.com';

INSERT INTO [Fiesta_Fries_DB].[Persona] (id, firstName, secondName, birthDate,direction, personalPhone, homePhone, uniqueUser, personType, IsDeleted)
VALUES (999999999, 'Selenium', 'test', '1985-11-17', 'San Jose, Costa Rica', '8888-1111', NULL, @userGuid, 'Empleador', 0);


INSERT INTO [Fiesta_Fries_DB].[Empresa] (CedulaJuridica, Nombre, DueñoEmpresa, Telefono, DireccionEspecifica, NoMaxBeneficios, DiaPago, FrecuenciaPago, FechaCreacion)
VALUES (9999999999, 'Empresa SeleniumTest', 999999999, 22243333, 'San Jose, Centro Empresarial', 3, 1, 'Mensual', '2025-01-01');

