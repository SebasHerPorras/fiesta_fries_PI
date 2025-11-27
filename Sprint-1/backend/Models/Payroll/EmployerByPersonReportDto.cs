namespace backend.Models.Payroll
{
    public class EmployerByPersonReportDto
    {
        public string Nombre { get; set; }
        public int Cedula { get; set; }
        public DateTime? PeriodoPago { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal SalarioBruto { get; set; }
        public decimal CargasSocialesEmpleador { get; set; }
        public decimal DeduccionesVoluntarias { get; set; }
        public decimal CostoEmpleador { get; set; }

    }
}
