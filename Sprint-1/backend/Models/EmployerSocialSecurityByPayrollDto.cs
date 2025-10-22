namespace backend.Models
{
    public class EmployerSocialSecurityByPayrollDto
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public long EmployeeId { get; set; }
        public string ChargeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public long CedulaJuridicaEmpresa { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}