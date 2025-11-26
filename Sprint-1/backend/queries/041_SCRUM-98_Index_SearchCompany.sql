-- �ndice para b�squedas de empresas activas
CREATE INDEX IX_empresa_IsDeleted ON [Fiesta_Fries_DB].[Empresa](IsDeleted) 
WHERE IsDeleted = 0;
