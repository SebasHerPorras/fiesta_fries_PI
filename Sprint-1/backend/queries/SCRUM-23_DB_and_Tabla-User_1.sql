
-- Create database from proyect Fiesta Fries
CREATE DATABASE Fiesta_Fries_DB;
GO

-- Use the created database
USE Fiesta_Fries_DB;
GO

CREATE TABLE [User] (
	PK_User UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
	email nvarchar(50) not null,
	[password] nvarchar(50) not null, -- el password debe ser encriptado (hash)

	
)

ALTER TABLE [User] 
ALTER COLUMN email nvarchar(60) NOT NULL; -- Modificar el tamaño del campo email a 60 caracteres para el hash


select * from [User];