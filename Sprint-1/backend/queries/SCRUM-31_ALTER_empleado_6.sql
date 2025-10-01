USE Fiesta_Fries_DB;
GO

ALTER TABLE Empleado
ADD salary int NOT NULL DEFAULT 0;

ALTER TABLE Empleado
ADD hireDate date NOT NULL DEFAULT GETDATE();

ALTER TABLE Empleado
ADD department varchar(50) NOT NULL DEFAULT 'General';

ALTER TABLE Empleado
ADD idCompny BIGINT NULL, CONSTRAINT FK_Empleado_Empresa FOREIGN KEY(idCompny) REFERENCES Empresa(CedulaJuridica);