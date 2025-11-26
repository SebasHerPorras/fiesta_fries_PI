USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

-- Agregar columna para CedulaJuridicaEmpresa
ALTER TABLE [Fiesta_Fries_DB].[EmployerSocialSecurityByPayroll]
ADD CedulaJuridicaEmpresa BIGINT NOT NULL DEFAULT 0;
GO

-- Agregar foreign key constraint
ALTER TABLE [Fiesta_Fries_DB].[EmployerSocialSecurityByPayroll]
ADD CONSTRAINT FK_EmployerSocialSecurityByPayroll_Empresa 
FOREIGN KEY (CedulaJuridicaEmpresa) REFERENCES Empresa(CedulaJuridica);
GO

USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO