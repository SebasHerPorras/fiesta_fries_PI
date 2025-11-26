-- Índice para búsquedas de empresas activas
CREATE INDEX IX_empresa_IsDeleted ON empresa(IsDeleted) 
WHERE IsDeleted = 0;
