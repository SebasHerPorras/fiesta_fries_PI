using backend.Models;
using backend.Interfaces;

namespace backend.Services
{
    public class CalculatorDeductionsEmployerService : ICalculatorDeductionsEmployerService
    {
        private readonly List<EmployerSocialSecurityContributions> _cargasSociales;
        private readonly EmployerSocialSecurityContributionsService _cargasSocialesService;
        private readonly EmployerSocialSecurityByPayrollService _payrollService;

        public CalculatorDeductionsEmployerService()
        {
            _cargasSocialesService = new EmployerSocialSecurityContributionsService();
            _cargasSociales = _cargasSocialesService.GetActiveContributions();
            _payrollService = new EmployerSocialSecurityByPayrollService();
        }

        public decimal CalculateEmployerDeductions(EmployeeCalculationDto empleado, int idReporte, long cedulaJuridicaEmpresa)
        {
            if (empleado == null)
                throw new ArgumentException("Los datos del empleado son requeridos");

            if (empleado.SalarioBruto <= 0)
                throw new ArgumentException("El salario bruto debe ser mayor a cero");

            if (_cargasSociales == null || !_cargasSociales.Any())
            {
                throw new ArgumentException("La lista de cargas sociales es requerida y no puede estar vacia");
            }

            var deducciones = new List<EmployerSocialSecurityByPayrollDto>();
            decimal totalDeducciones = 0;

            foreach (var cargaSocial in _cargasSociales)
            {
                var monto = Math.Round(empleado.SalarioBruto * cargaSocial.Percentage, 2);

                deducciones.Add(new EmployerSocialSecurityByPayrollDto
                {
                    ReportId = idReporte,
                    EmployeeId = empleado.CedulaEmpleado,
                    ChargeName = cargaSocial.Name,
                    Amount = monto,
                    Percentage = cargaSocial.Percentage,
                    CedulaJuridicaEmpresa = cedulaJuridicaEmpresa
                });

                totalDeducciones += monto;
            }

            _payrollService.SaveEmployerDeductions(deducciones);

            return Math.Round(totalDeducciones, 2);
        }

        public List<EmployerSocialSecurityContributions> ObtenerCargasSocialesActuales()
        {
            return _cargasSociales.ToList();
        }
    }
}
