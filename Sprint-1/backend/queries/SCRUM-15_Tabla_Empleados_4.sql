USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearán bajo el schema Fiesta_Fries_DB
GO

CREATE TABLE [Fiesta_Fries_DB].[User](
id int Primary KEY NOT NULL,CONSTRAINT FK_Empleado_Persona FOREIGN KEY(id) REFERENCES Persona(id),
position varchar(50) NOT NULL,
employmentType varchar(30) NOT NULL CHECK (employmentType IN('Por horas','Tiempo completo','Medio tiempo')) default 'Tiempo completo',
)

CREATE TABLE [Fiesta_Fries_DB].[User](
token nvarchar(50) PRIMARY KEY NOT NULL,
expirationDate DATETIME NOT NULL,
)

