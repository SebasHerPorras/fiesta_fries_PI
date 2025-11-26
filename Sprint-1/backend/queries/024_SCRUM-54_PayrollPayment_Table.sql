USE [G02-2025-II-DB];
GO
-- Todas las tablas se crearÃ¡n bajo el schema Fiesta_Fries_DB
GO

CREATE TABLE [Fiesta_Fries_DB].[PayrollPayment](
    [PaymentId] [int] IDENTITY(2000,1) NOT NULL,     
    [PayrollId] [int] NOT NULL,                   
    [EmployeeId] [int] NOT NULL,                  
    [GrossSalary] [decimal](18,2) NOT NULL,       
    [DeductionsAmount] [decimal](18,2) NOT NULL,  
    [BenefitsAmount] [decimal](18,2) NOT NULL,    
    [NetSalary] [decimal](18,2) NOT NULL,        
    [PaymentDate] [datetime] NULL,               
    [Status] [nvarchar](20) NOT NULL DEFAULT 'PENDIENTE',
    CONSTRAINT [PK_PayrollPayment] PRIMARY KEY ([PaymentId]),
    CONSTRAINT [FK_PayrollPayment_Payroll] FOREIGN KEY ([PayrollId]) 
        REFERENCES [Fiesta_Fries_DB].[Payroll] ([PayrollId]),
    CONSTRAINT [FK_PayrollPayment_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [Fiesta_Fries_DB].[Empleado] ([id])
)

--b�squedas por empleado-planilla
CREATE UNIQUE INDEX [IX_PayrollPayment_EmployeePayroll] 
ON [Fiesta_Fries_DB].[PayrollPayment] ([EmployeeId], [PayrollId])