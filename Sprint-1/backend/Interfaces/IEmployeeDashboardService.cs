// IEmployeeDashboardService interface
using System;
using System.Collections.Generic;
using backend.Models;


namespace backend.Interfaces
{
    public interface IEmployeeDashboardService
    {
        EmployeeDashboardDataModelcs? GetDashboardData(int id, DateTime date);
        List<EmployeeDeductionsByPayrollModel> GetEmployeePayrollData(int id, DateTime date);
        EmployeeDashboardDataModelcs BuildPayrollData(int employeeSalary, decimal deductionsAmount, decimal netsalary, decimal retained_percentage);
    }
}
