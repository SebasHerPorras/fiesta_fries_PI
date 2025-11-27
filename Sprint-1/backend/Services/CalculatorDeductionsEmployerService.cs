using backend.Models;
using backend.Interfaces;

namespace backend.Services
{
    public class CalculatorDeductionsEmployerService : ICalculatorDeductionsEmployerService
    {
        private readonly List<EmployerSocialSecurityContributions> _cargasSociales;
        private readonly IEmployerSocialSecurityContributionsService _cargasSocialesService;
        private readonly IEmployerSocialSecurityByPayrollService _payrollService;
        private readonly bool _saveInDB;

        public CalculatorDeductionsEmployerService(
            IEmployerSocialSecurityContributionsService cargasSocialesService,
            IEmployerSocialSecurityByPayrollService payrollService,
            bool saveInDB = true)
        {
            _cargasSocialesService = cargasSocialesService;
            _cargasSociales = _cargasSocialesService.GetActiveContributions();
            _payrollService = payrollService;
            _saveInDB = saveInDB;
        }

        public decimal CalculateEmployerDeductions(EmployeeCalculationDto empleado, int idReporte, long cedulaJuridicaEmpresa)
        {
            if (empleado == null)
                throw new ArgumentException("Los datos del empleado son requeridos");

            if (empleado.SalarioBruto <= 0)
                throw new ArgumentException("El salario bruto debe ser mayor a cero");

            // Verificar si es empleado "Por horas" - NO aplican cargas sociales del empleador
            if (string.Equals(empleado.TipoEmpleado?.Trim(), "Por horas", StringComparison.OrdinalIgnoreCase))
            {
                // Registrar que no aplican deducciones para este tipo de empleado
                var deduccionesSinCargo = new List<EmployerSocialSecurityByPayrollDto>
                {
                    new EmployerSocialSecurityByPayrollDto
                    {
                        ReportId = idReporte,
                        EmployeeId = empleado.CedulaEmpleado,
                        ChargeName = "Sin cargas sociales - Empleado por horas",
                        Amount = 0,
                        Percentage = 0,
                        CedulaJuridicaEmpresa = cedulaJuridicaEmpresa
                    }
                };

                if (_saveInDB)
                {
                    try
                    {
                        _payrollService.SaveEmployerDeductions(deduccionesSinCargo);
                    }
                    catch (Exception)
                    {
                        // Error al guardar, pero continuar
                    }
                }
                return 0;
            }

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

            if (_saveInDB)
            {
                try
                {
                    _payrollService.SaveEmployerDeductions(deducciones);
                }
                catch (Exception)
                {
                    // Error al guardar, pero continuar con el cálculo
                    // El valor calculado es correcto independientemente del guardado
                }
            }

            return Math.Round(totalDeducciones, 2);
        }

        public List<EmployerSocialSecurityContributions> ObtenerCargasSocialesActuales()
        {
            return _cargasSociales.ToList();
        }
    }
}
