
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
	active BIT NOT NULL DEFAULT 0

	-- SCRUM:7_crear_tabla_Usuario_2
)

ALTER TABLE [User] 
ALTER COLUMN email nvarchar(60) NOT NULL; -- Modificar el tamaño del campo email a 60 caracteres para el hash

INSERT INTO [User] (email, password) VALUES 
('sebastian.hernandezporras@ucr.ac.cr', 'password123'),
('emilio.romero@ucr.ac.cr', 'password123'),
('diego.cerdasdelgado@ucr.ac.cr', 'password123'),
('enrique.bermudez@ucr.ac.cr', 'password123');

UPDATE [User]
SET [password] = 'Hola.2025'
WHERE email = 'sebastian.hernandezporras@ucr.ac.cr';

UPDATE [User]
SET [password] = 'Hola.2025'
WHERE email = 'emilio.romero@ucr.ac.cr';

UPDATE [User]
SET [password] = 'Hola.2025'
WHERE email = 'diego.cerdasdelgado@ucr.ac.cr';

UPDATE [User]
SET [password] = 'Hola.2025'
WHERE email = 'enrique.bermudez@ucr.ac.cr';

select * from [User];


	

CREATE TABLE Persona(
id int Primary KEY NOT NULL,
[firstName] varchar(50) NOT NULL,
[secondName] varchar(50) NOT NULL, 
birthdate date NOT NULL,
direction varchar(200) NOT NULL,
personalPhone int NOT NULL,
homePhone int NULL,
uniqueUser UNIQUEIDENTIFIER NOT NULL,CONSTRAINT FK_Persona_Usuario FOREIGN KEY(uniqueUser) REFERENCES [User](PK_User),
personType varchar(30) NOT NULL CHECK (personType IN('Empleador','Empleado')),
)

INSERT INTO [User](email,[password]) VALUES
('cerdasignacio5@gmail.com','ven bailalo')

select * from [User];



INSERT INTO Persona(id,[firstName],[secondName],birthdate,direction,personalPhone,homePhone,uniqueUser,personType) VALUES
(119180741,'Diego','Cerdas Delgado','2004-10-10','palo de mangos',64174682,NULL,'D98BC9C9-2FA7-4F95-B4BD-AAC1AEC7C539','Empleador')

select * from Persona;

select* from [User];

CREATE TABLE EmailVerification(
   userId UNIQUEIDENTIFIER NOT NULL ,Constraint FK_User_Notification FOREIGN KEY(userId) REFERENCES [User](PK_User),
   token nvarchar(200),
   experationDate DATETIME NOT NULL,
);

INSERT INTO EmailVerification(userID,token,experationDate) VALUES
('039CE7D8-9109-4D13-9CC7-201211C3CA85','pepe','2025-10-10')
