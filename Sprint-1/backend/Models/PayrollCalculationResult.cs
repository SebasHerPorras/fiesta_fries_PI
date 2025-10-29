using backend.Models;
using backend.Models.Payroll; 

namespace backend.Models.Payroll.Results
{
    public class PayrollCalculationResult
    {
        public List<EmployeeCalculation> EmployeeCalculations { get; set; } = new(); // Cambiar a set
        public decimal TotalDeductions => EmployeeCalculations.Sum(x => x.Deductions);
        public decimal TotalBenefits => EmployeeCalculations.Sum(x => x.Benefits);
        public decimal TotalAmount => TotalDeductions + TotalBenefits;
        public int ProcessedEmployees => EmployeeCalculations.Count;

    
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalEmployeeDeductions { get; set; }
        public decimal TotalEmployerDeductions { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalEmployerCost { get; set; }

        public void AddEmployeeCalculation(EmployeeCalculation calculation)
        {
            EmployeeCalculations.Add(calculation);
        }

        public List<PayrollPayment> ToPayments(int payrollId)
        {
            return EmployeeCalculations
                .Select(calc => calc.ToPayment(payrollId))
                .ToList();
        }
    }

    public record EmployeeCalculation(
        EmpleadoModel Employee,
        decimal Deductions,
        decimal Benefits)
    {
        public decimal NetSalary => Employee.salary - Deductions + Benefits;

        public PayrollPayment ToPayment(int payrollId) => new()
        {
            PayrollId = payrollId,
            EmployeeId = Employee.id,
            GrossSalary = Employee.salary,
            DeductionsAmount = Deductions,
            BenefitsAmount = Benefits,
            NetSalary = NetSalary,
            PaymentDate = DateTime.Now,
            Status = "PROCESADO"
        };
    }
}

