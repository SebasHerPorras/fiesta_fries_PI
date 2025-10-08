
USE Fiesta_Fries_DB;
GO
-- Tabla Empresa

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

SELECT * FROM Empresa;