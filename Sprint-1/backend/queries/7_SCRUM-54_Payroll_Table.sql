USE Fiesta_Fries_DB;
GO

CREATE TABLE [dbo].[Payroll](
    [PayrollId] [int] IDENTITY(1000,1) NOT NULL,    
    [PeriodDate] [datetime] NOT NULL,            
    [CompanyId] [bigint] NOT NULL,               
    [IsCalculated] [bit] NOT NULL DEFAULT 0,      
    [TotalAmount] [decimal](18,2) NULL,          
    [ApprovedBy] [nvarchar](100) NULL,            
    [LastModified] [datetime] NOT NULL DEFAULT GETDATE(),
    CONSTRAINT [PK_Payroll] PRIMARY KEY ([PayrollId])
)

--para evitar duplicados por período/empresa
CREATE UNIQUE INDEX [IX_Payroll_PeriodCompany] 
ON [dbo].[Payroll] ([PeriodDate], [CompanyId])