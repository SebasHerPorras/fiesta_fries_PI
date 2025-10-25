namespace backend.Models.Payroll.Results
{
    public class PayrollCalculationResult
    {
        public List<EmployeeCalculation> EmployeeCalculations { get; } = new();
        public decimal TotalDeductions => EmployeeCalculations.Sum(x => x.Deductions);
        public decimal TotalBenefits => EmployeeCalculations.Sum(x => x.Benefits);
        public decimal TotalTax => EmployeeCalculations.Sum(x => x.Tax);
        public decimal TotalAmount => TotalDeductions + TotalBenefits + TotalTax;
        public int ProcessedEmployees => EmployeeCalculations.Count;

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
        decimal Benefits,
        decimal Tax)
    {
        public decimal NetSalary => Employee.salary - Deductions + Benefits - Tax;

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