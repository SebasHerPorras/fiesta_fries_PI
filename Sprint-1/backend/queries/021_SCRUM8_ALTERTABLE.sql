USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

ALTER TABLE [Fiesta_Fries_DB].[Empresa] ADD [FechaCreacion] DATETIME NOT NULL;
select * FROM [Fiesta_Fries_DB].[Empresa];