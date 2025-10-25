using backend.Models;
using backend.Interfaces;

namespace backend.Services
{
    public class CalculatorDeductionsEmployeeService : ICalculatorDeductionsEmployeeService
    {
        private readonly List<EmployeeSocialSecurityContributions> _deduccionesSociales;
        private readonly List<PersonalIncomeTax> _escalasImpuesto;
        private readonly IEmployeeSocialSecurityContributionsService _deduccionesSocialesService;
        private readonly IPersonalIncomeTaxService _impuestoRentaService;
        private readonly IEmployeeDeductionsByPayrollService _payrollService;

        public CalculatorDeductionsEmployeeService(
            IEmployeeSocialSecurityContributionsService deduccionesSocialesService,
            IPersonalIncomeTaxService impuestoRentaService,
            IEmployeeDeductionsByPayrollService payrollService)
        {
            _deduccionesSocialesService = deduccionesSocialesService;
            _impuestoRentaService = impuestoRentaService;
            _payrollService = payrollService;
            
            _deduccionesSociales = _deduccionesSocialesService.GetActiveContributions();
            _escalasImpuesto = _impuestoRentaService.GetActiveScales();
        }

        public decimal CalculateEmployeeDeductions(EmployeeCalculationDto empleado, int idReporte, long cedulaJuridicaEmpresa)
        {
            if (empleado == null)
                throw new ArgumentException("Los datos del empleado son requeridos");

            if (empleado.SalarioBruto <= 0)
                throw new ArgumentException("El salario bruto debe ser mayor a cero");

            var deducciones = new List<EmployeeDeductionsByPayrollDto>();
            decimal totalDeducciones = 0;

            // Verificar si es empleado "Por horas" (servicio profesional) - NO aplican deducciones
            if (string.Equals(empleado.TipoEmpleado?.Trim(), "Por horas", StringComparison.OrdinalIgnoreCase))
            {
                // Para empleados por horas no se aplican deducciones de CCSS ni impuesto sobre la renta
                // Solo guardamos un registro informativo
                deducciones.Add(new EmployeeDeductionsByPayrollDto
                {
                    ReportId = idReporte,
                    EmployeeId = (int)empleado.CedulaEmpleado,
                    CedulaJuridicaEmpresa = cedulaJuridicaEmpresa,
                    DeductionName = "Sin deducciones - Empleado por horas",
                    DeductionAmount = 0,
                    Percentage = null
                });

                _payrollService.SaveEmployeeDeductions(deducciones);
                return 0;
            }

            // 1. Calcular deducciones CCSS (5.5% salud + 4% pensiones) - Solo para empleados regulares
            foreach (var deduccionSocial in _deduccionesSociales)
            {
                var monto = Math.Round(empleado.SalarioBruto * deduccionSocial.Percentage, 2);

                deducciones.Add(new EmployeeDeductionsByPayrollDto
                {
                    ReportId = idReporte,
                    EmployeeId = (int)empleado.CedulaEmpleado,
                    CedulaJuridicaEmpresa = cedulaJuridicaEmpresa,
                    DeductionName = deduccionSocial.Name,
                    DeductionAmount = monto,
                    Percentage = deduccionSocial.Percentage
                });

                totalDeducciones += monto;
            }

            // 2. Calcular impuesto sobre la renta - Solo para empleados regulares
            var impuestoRenta = CalculateIncomeTax(empleado.SalarioBruto);
            if (impuestoRenta > 0)
            {
                deducciones.Add(new EmployeeDeductionsByPayrollDto
                {
                    ReportId = idReporte,
                    EmployeeId = (int)empleado.CedulaEmpleado,
                    CedulaJuridicaEmpresa = cedulaJuridicaEmpresa,
                    DeductionName = "Impuesto sobre la Renta",
                    DeductionAmount = impuestoRenta,
                    Percentage = null // No aplica porcentaje fijo
                });

                totalDeducciones += impuestoRenta;
            }

            // 3. Guardar en base de datos
            _payrollService.SaveEmployeeDeductions(deducciones);

            return Math.Round(totalDeducciones, 2);
        }

        private decimal CalculateIncomeTax(decimal grossSalary)
        {
            if (_escalasImpuesto == null || !_escalasImpuesto.Any())
                return 0;

            // Encontrar la escala correcta
            var escala = _escalasImpuesto
                .Where(e => grossSalary >= e.MinAmount && (e.MaxAmount == null || grossSalary <= e.MaxAmount))
                .FirstOrDefault();

            if (escala == null || escala.Percentage == 0)
                return 0;

            // Calcular impuesto: (salario - monto_minimo) * porcentaje + base_amount
            var montoGravable = grossSalary - escala.MinAmount;
            var impuestoCalculado = (montoGravable * escala.Percentage) + escala.BaseAmount;

            return Math.Round(impuestoCalculado, 2);
        }

        public List<EmployeeSocialSecurityContributions> ObtenerDeduccionesSocialesActuales()
        {
            return _deduccionesSociales.ToList();
        }

        public List<PersonalIncomeTax> ObtenerEscalasImpuestoRenta()
        {
            return _escalasImpuesto.ToList();
        }
    }
}