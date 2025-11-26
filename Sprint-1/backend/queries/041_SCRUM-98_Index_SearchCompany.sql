-- �ndice para b�squedas de empresas activas
use [G02-2025-II-DB];

CREATE INDEX IX_empresa_IsDeleted ON [Fiesta_Fries_DB].[Empresa](IsDeleted) 
WHERE IsDeleted = 0;
