namespace backend.Models.Payroll
{
    public class PayrollEmployeeReport
    {
        public EmployeeReportHeader Header { get; set; } = new();
        public List<EmployeeDeductionItem> EmployeeDeductions { get; set; } = new();
        public decimal TotalEmployeeDeductions { get; set; }
        public List<EmployerBenefitItem> EmployerBenefits { get; set; } = new();
        public decimal TotalEmployerBenefits { get; set; }
        public EmployeeReportTotals Totals { get; set; } = new();
    }

    public class EmployeeReportHeader
    {
        public long CompanyId { get; set; }
        public string NombreEmpresa { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string NombreEmpleado { get; set; } = string.Empty;
        public string TipoEmpleado { get; set; } = string.Empty;
        public DateTime FechaPago { get; set; }
        public decimal SalarioBruto { get; set; }
    }

    public class EmployeeDeductionItem
    {
        public string DeductionName { get; set; } = string.Empty;
        public decimal DeductionAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class EmployerBenefitItem
    {
        public string BenefitName { get; set; } = string.Empty;
        public string BenefitType { get; set; } = string.Empty;
        public decimal BenefitAmount { get; set; }
        public decimal Percentage { get; set; }
    }

    public class EmployeeReportTotals
    {
        public decimal TotalDeduccionesObligatorias { get; set; }
        public decimal TotalBeneficiosVoluntarios { get; set; }
        public decimal TotalDeducciones { get; set; }
        public decimal PagoNeto { get; set; }
    }
}