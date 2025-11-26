
USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO
-- Tabla Empresa

CREATE TABLE [Fiesta_Fries_DB].[Empresa](
    [CedulaJuridica] BIGINT NOT NULL, 
    [Nombre] VARCHAR(100) NOT NULL,
    [DueñoEmpresa] INT NOT NULL,
    [Telefono] INT NULL, 
    [DireccionEspecifica] VARCHAR(200) NULL, 
    [NoMaxBeneficios] INT NOT NULL,
    [DiaPago] SMALLINT NOT NULL,
    [FrecuenciaPago] VARCHAR(30) NOT NULL,
    [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(), 
    CONSTRAINT [PK_Empresa] PRIMARY KEY CLUSTERED 
    (
        [CedulaJuridica] ASC
    )
);
GO

-- AGREGAR FOREIGN KEY
ALTER TABLE [Fiesta_Fries_DB].[Empresa]  
    WITH CHECK ADD CONSTRAINT [FK_Empresa_Persona] 
    FOREIGN KEY([DueñoEmpresa])
    REFERENCES [dbo].[Persona] ([id]);
GO



ALTER TABLE [Fiesta_Fries_DB].[Empresa] 
    CHECK CONSTRAINT [FK_Empresa_Persona];
GO

select * FROM [Fiesta_Fries_DB].[Persona];