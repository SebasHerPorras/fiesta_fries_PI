use Fiesta_Fries_DB;

CREATE NONCLUSTERED INDEX IX_EmployeeDeductions_Filtering
ON EmployeeDeductionsByPayroll (EmployeeId, DeductionName, CreatedDate);
