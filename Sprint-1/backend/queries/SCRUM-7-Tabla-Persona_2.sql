USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearán bajo el schema Fiesta_Fries_DB
GO

ALTER TABLE [Fiesta_Fries_DB].[Persona]
ADD active BIT NOT NULL CONSTRAINT DF_User_Active DEFAULT 0;
ALTER TABLE [Fiesta_Fries_DB].[Persona]
ADD [admin] BIT NOT NULL CONSTRAINT DF_User_Admin DEFAULT 0;

CREATE TABLE [Fiesta_Fries_DB].[User](
id int Primary KEY NOT NULL,
[firstName] varchar(50) NOT NULL,
[secondName] varchar(50) NOT NULL, 
birthdate date NOT NULL,
direction varchar(200) NOT NULL,
personalPhone varchar(30) NOT NULL,
homePhone varchar(30) NULL,
uniqueUser UNIQUEIDENTIFIER NOT NULL,CONSTRAINT FK_Persona_Usuario FOREIGN KEY(uniqueUser) REFERENCES [User](PK_User),
personType varchar(30) NOT NULL CHECK (personType IN('Empleador','Empleado')),
)

select * FROM [Fiesta_Fries_DB].[Persona];

select * FROM [Fiesta_Fries_DB].[Persona];

CREATE TABLE [Fiesta_Fries_DB].[User](
   userId UNIQUEIDENTIFIER NOT NULL ,Constraint FK_User_Notification FOREIGN KEY(userId) REFERENCES [User](PK_User),
   token nvarchar(200),
   experationDate DATETIME NOT NULL,
);
