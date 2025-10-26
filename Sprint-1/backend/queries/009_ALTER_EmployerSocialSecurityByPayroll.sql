USE Fiesta_Fries_DB;
GO

-- Agregar columna para CedulaJuridicaEmpresa
ALTER TABLE EmployerSocialSecurityByPayroll
ADD CedulaJuridicaEmpresa BIGINT NOT NULL DEFAULT 0;
GO

-- Agregar foreign key constraint
ALTER TABLE EmployerSocialSecurityByPayroll
ADD CONSTRAINT FK_EmployerSocialSecurityByPayroll_Empresa 
FOREIGN KEY (CedulaJuridicaEmpresa) REFERENCES Empresa(CedulaJuridica);
GO

USE Fiesta_Fries_DB;
GO