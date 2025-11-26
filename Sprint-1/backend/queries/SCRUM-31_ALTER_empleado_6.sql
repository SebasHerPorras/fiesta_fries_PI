USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearán bajo el schema Fiesta_Fries_DB
GO

ALTER TABLE [Fiesta_Fries_DB].[Empleado]
ADD salary int NOT NULL DEFAULT 0;

ALTER TABLE [Fiesta_Fries_DB].[Empleado]
ADD hireDate date NOT NULL DEFAULT GETDATE();

ALTER TABLE [Fiesta_Fries_DB].[Empleado]
ADD department varchar(50) NOT NULL DEFAULT 'General';

ALTER TABLE [Fiesta_Fries_DB].[Empleado]
ADD idCompny BIGINT NULL, CONSTRAINT FK_Empleado_Empresa FOREIGN KEY(idCompny) REFERENCES Empresa(CedulaJuridica);

SELECT* FROM [Fiesta_Fries_DB].[Empleado];

/*
DELETE FROM [Fiesta_Fries_DB].[User];
DELETE FROM [Fiesta_Fries_DB].[Empleado];
DELETE FROM [Fiesta_Fries_DB].[Empleado];
DELETE FROM [Fiesta_Fries_DB].[Empleado];
DELETE FROM [Fiesta_Fries_DB].[Empleado];
*/