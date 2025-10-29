USE Fiesta_Fries_DB;
GO

CREATE TABLE [dbo].[PayrollPayment](
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
        REFERENCES [dbo].[Payroll] ([PayrollId]),
    CONSTRAINT [FK_PayrollPayment_Employee] FOREIGN KEY ([EmployeeId]) 
        REFERENCES [dbo].[Empleado] ([id])
)

--búsquedas por empleado-planilla
CREATE UNIQUE INDEX [IX_PayrollPayment_EmployeePayroll] 
ON [dbo].[PayrollPayment] ([EmployeeId], [PayrollId])