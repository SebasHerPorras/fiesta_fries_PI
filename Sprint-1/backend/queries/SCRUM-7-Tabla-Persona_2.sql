USE Fiesta_Fries_DB;
GO

ALTER TABLE [User]
ADD active BIT NOT NULL CONSTRAINT DF_User_Active DEFAULT 0;
ALTER TABLE [User]
ADD [admin] BIT NOT NULL CONSTRAINT DF_User_Admin DEFAULT 0;

CREATE TABLE Persona(
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
