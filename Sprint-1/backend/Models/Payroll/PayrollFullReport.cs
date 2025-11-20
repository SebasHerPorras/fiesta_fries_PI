namespace backend.Models.Payroll
{
    public class PayrollFullReport
    {
        public PayrollReportHeader Header { get; set; } = new();
        public List<EmployeeDetail> Employees { get; set; } = new();
        public List<ChargeDetail> EmployerCharges { get; set; } = new();
        public List<DeductionDetail> EmployeeDeductions { get; set; } = new();
        public List<BenefitDetail> Benefits { get; set; } = new();
    }

    public class PayrollReportHeader
    {
        public int PayrollId { get; set; }
        public DateTime PeriodDate { get; set; }
        public long CompanyId { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public string NombreEmpleador { get; set; } = string.Empty;
        public int DiaPago { get; set; }
        public string FrecuenciaPago { get; set; } = string.Empty;
        public decimal TotalGrossSalary { get; set; }
        public decimal TotalEmployeeDeductions { get; set; }
        public decimal TotalEmployerDeductions { get; set; }
        public decimal TotalBenefits { get; set; }
        public decimal TotalNetSalary { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class EmployeeDetail
    {
        public int EmployeeId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string TipoEmpleado { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
        public decimal DeductionsAmount { get; set; }
        public decimal BenefitsAmount { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ChargeDetail
    {
        public string ChargeName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PercentageDisplay { get; set; }
    }

    public class DeductionDetail
    {
        public string DeductionName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PercentageDisplay { get; set; }
    }

    public class BenefitDetail
    {
        public string BenefitName { get; set; } = string.Empty;
        public string BenefitType { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}