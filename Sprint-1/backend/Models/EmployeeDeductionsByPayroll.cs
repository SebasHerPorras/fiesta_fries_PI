namespace backend.Models
{
    public class EmployeeDeductionsByPayrollModel
    {
        public int Id { get; set; }
        public int ReportId { get; set; }

        public long EmployeeId { get; set; }

        public long CedulaJuridicaEmpresa { get; set; }
        public string DeductionName { get; set; } = string.Empty;

        public decimal  DeductionAmount { get; set; }
        public decimal? Percentage { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}