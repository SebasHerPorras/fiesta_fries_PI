
-- Create database from proyect Fiesta Fries
-- CREATE DATABASE ya existe: G02-2025-II-DB
-- Schema: Fiesta_Fries_DB
GO

-- Use the created database
USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

CREATE TABLE [Fiesta_Fries_DB].[User] (
	PK_User UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	email nvarchar(50) not null,
	[password] nvarchar(50) not null, -- el password debe ser encriptado (hash)

	
)

ALTER TABLE [Fiesta_Fries_DB].[User] 
ALTER COLUMN email nvarchar(60) NOT NULL; -- Modificar el tamaño del campo email a 60 caracteres para el hash


select * FROM [Fiesta_Fries_DB].[User];