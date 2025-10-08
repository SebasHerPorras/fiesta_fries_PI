use Fiesta_Fries_DB
GO

CREATE TABLE Beneficio (
    IdBeneficio INT PRIMARY KEY IDENTITY(1,1),  
    CedulaJuridica BIGINT NOT NULL,       
    Nombre NVARCHAR(100) NOT NULL,          
    Tipo NVARCHAR(20) NOT NULL CHECK (Tipo IN ('Monto Fijo', 'Porcentual', 'API')), 
    QuienAsume NVARCHAR(50) NOT NULL,       
    Valor DECIMAL(10,2) NULL,    
    Etiqueta NVARCHAR(20) NOT NULL CHECK (Etiqueta IN ('Beneficio', 'Deducción')),
    CONSTRAINT FK_Beneficio_Empresa FOREIGN KEY (CedulaJuridica)
        REFERENCES Empresa(CedulaJuridica)
        ON DELETE CASCADE
);