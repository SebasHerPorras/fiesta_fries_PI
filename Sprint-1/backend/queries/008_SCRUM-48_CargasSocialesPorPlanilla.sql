USE Fiesta_Fries_DB;
GO

-- Table to store employer social security charges by payroll
CREATE TABLE EmployerSocialSecurityByPayroll (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ReportId INT NOT NULL,
    EmployeeId int NOT NULL,
    ChargeName VARCHAR(100) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    Percentage DECIMAL(5,4) NOT NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    
    FOREIGN KEY (EmployeeId) REFERENCES Empleado(id)
);
GO