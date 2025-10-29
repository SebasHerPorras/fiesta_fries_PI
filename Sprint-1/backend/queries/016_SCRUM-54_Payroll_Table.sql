USE Fiesta_Fries_DB;
GO

CREATE TABLE [dbo].[Payroll] (
    [PayrollId] INT IDENTITY(1000,1) NOT NULL,         
    [PeriodDate] DATETIME NOT NULL,                     
    [CompanyId] BIGINT NOT NULL,                        
    [IsCalculated] BIT NOT NULL DEFAULT 0,             
    [ApprovedBy] NVARCHAR(100) NULL,                   
    [LastModified] DATETIME NOT NULL DEFAULT GETDATE(),
    [TotalGrossSalary] DECIMAL(18,2) NULL,           
    [TotalEmployerDeductions] DECIMAL(18,2) NULL,      
    [TotalEmployeeDeductions] DECIMAL(18,2) NULL,    
    [TotalBenefits] DECIMAL(18,2) NULL,                
    [TotalNetSalary] DECIMAL(18,2) NULL,               
    [TotalEmployerCost] DECIMAL(18,2) NULL,            
    CONSTRAINT [PK_Payroll] PRIMARY KEY ([PayrollId])
);
GO

--para evitar duplicados por periodo y empresa
CREATE UNIQUE INDEX [IX_Payroll_PeriodCompany]
ON [dbo].[Payroll] ([PeriodDate], [CompanyId]);
GO
