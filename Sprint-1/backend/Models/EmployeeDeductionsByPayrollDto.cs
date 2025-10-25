namespace backend.Models
{
    public class EmployeeDeductionsByPayrollDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public int EmployeeId { get; set; }
        public long CedulaJuridicaEmpresa { get; set; }
        public string DeductionName { get; set; } = string.Empty;
        public decimal DeductionAmount { get; set; }
        public decimal? Percentage { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}