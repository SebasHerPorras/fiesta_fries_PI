using backend.Interfaces;
using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public class EmployeeDashboardServiceTest : IEmployeeDashboardService
    {
        private readonly Dictionary<int, int> _salaries = new();
        private readonly List<EmployeeDeductionsByPayrollModel> _payrolls = new();

        public void AddSalary(int id, int salary)
        {
            _salaries[id] = salary;
        }

    
        public void AddPayrollDeduction(EmployeeDeductionsByPayrollModel model)
        {
            _payrolls.Add(model);
        }

        public int GetSalaryAmount(int id)
        {
            return _salaries.ContainsKey(id) ? _salaries[id] : 0;
        }

        public List<EmployeeDeductionsByPayrollModel> GetEmployeePayrollData(int id, DateTime date)
        {
            return _payrolls
                .Where(p => p.EmployeeId == id && p.CreatedDate.Date == date.Date)
                .ToList();
        }

        public decimal DeductionAmount(List<EmployeeDeductionsByPayrollModel> payrollData)
        {
            return payrollData.Sum(x => x.DeductionAmount);
        }

        public EmployeeDashboardDataModelcs? GetDashboardData(int id, DateTime date)
        {
            int salary = GetSalaryAmount(id);
            var payrollData = GetEmployeePayrollData(id, date);
            decimal totalDeductions = DeductionAmount(payrollData);

            decimal netSalary = salary - totalDeductions;

            decimal retained_percentage =
                salary == 0 ? 0 : 100 - ((netSalary / salary) * 100);

            return BuildPayrollData(salary, totalDeductions, netSalary, retained_percentage);
        }

        public EmployeeDashboardDataModelcs BuildPayrollData(
            int employeeSalary,
            decimal deductionsAmount,
            decimal netsalary,
            decimal retained_percentage
        )
        {
            return new EmployeeDashboardDataModelcs
            {
                CrudSalary = employeeSalary,
                TotalDeductions = deductionsAmount,
                NetSalary = netsalary,
                ReteinedPercentage = retained_percentage
            };
        }
    }
}
