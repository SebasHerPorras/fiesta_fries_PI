USE Fiesta_Fries_DB;
GO

CREATE TABLE Empleado(
id int Primary KEY NOT NULL,CONSTRAINT FK_Empleado_Persona FOREIGN KEY(id) REFERENCES Persona(id),
position varchar(50) NOT NULL,
employmentType varchar(30) NOT NULL CHECK (employmentType IN('Por horas','Tiempo completo','Medio tiempo')) default 'Tiempo completo',
)

CREATE TABLE EmailVerificationE(
token nvarchar(50) PRIMARY KEY NOT NULL,
expirationDate DATETIME NOT NULL,
)


SELECT* FROM EmailVerificationE;
SELECT* FROM EmailVerificationE;