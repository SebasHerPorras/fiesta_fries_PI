
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

-- =============================================
-- Tabla Empresa
-- =============================================

CREATE TABLE [dbo].[Empresa](
    [CedulaJuridica] BIGINT NOT NULL, 
    [Nombre] VARCHAR(100) NOT NULL,
    [DueñoEmpresa] INT NOT NULL,
    [Telefono] INT NULL, 
    [DireccionEspecifica] VARCHAR(200) NULL, 
    [NoMaxBeneficios] INT NOT NULL,
    [DiaPago] SMALLINT NOT NULL,
    [FrecuenciaPago] VARCHAR(30) NOT NULL,
    CONSTRAINT [PK_Empresa] PRIMARY KEY CLUSTERED 
    (
        [CedulaJuridica] ASC
    )
);
GO


-- AGREGAR FOREIGN KEY
ALTER TABLE [dbo].[Empresa]  
    WITH CHECK ADD CONSTRAINT [FK_Empresa_Persona] 
    FOREIGN KEY([DueñoEmpresa])
    REFERENCES [dbo].[Persona] ([id]);
GO

ALTER TABLE [dbo].[Empresa] 
    CHECK CONSTRAINT [FK_Empresa_Persona];
GO

-- VERIFICAR
SELECT * FROM Empresa;