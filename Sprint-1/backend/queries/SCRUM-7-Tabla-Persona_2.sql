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

select * from [User];

select * from Persona;

CREATE TABLE EmailVerification(
   userId UNIQUEIDENTIFIER NOT NULL ,Constraint FK_User_Notification FOREIGN KEY(userId) REFERENCES [User](PK_User),
   token nvarchar(200),
   experationDate DATETIME NOT NULL,
);
