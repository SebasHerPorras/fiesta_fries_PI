namespace backend.Models.Payroll
{
    public class EmployerHistoricalReportDto
    {
        public string Nombre { get; set; } 
        public string FrecuenciaPago { get; set; }
        public DateTime? PeriodoPago { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal SalarioBruto { get; set; }
        public decimal CargasSocialesEmpleador { get; set; }
        public decimal DeduccionesVoluntarias { get; set; }
        public decimal CostoEmpleador { get; set; }

        // Texto para ensamblaje de respuesta final (Total CRC + num)
        public string SalarioBrutoText { get; set; }
        public string CargasSocialesEmpleadorText { get; set; }
        public string DeduccionesVoluntariasText { get; set; }
        public string CostoEmpleadorText { get; set; }
    }
}
