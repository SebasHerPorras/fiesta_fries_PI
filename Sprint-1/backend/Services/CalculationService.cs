using backend.Interfaces;
using backend.Interfaces.Services;
using backend.Models;
using backend.Repositories;

namespace backend.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ICalculatorDeductionsEmployerService _deductionsService;
        private readonly ICalculatorDeductionsEmployeeService _employeeDeductionsService;
        private readonly IPersonalIncomeTaxService _incomeTaxService;
        private readonly ICalculatorBenefitsService _benefitsService;
        private readonly ILogger<CalculationService> _logger;

        public CalculationService(
            ICalculatorDeductionsEmployerService deductionsService,
            ICalculatorDeductionsEmployeeService employeeDeductionsService,
            IPersonalIncomeTaxService incomeTaxService,
            ICalculatorBenefitsService benefitsService,
            ILogger<CalculationService> logger)
        {
            _deductionsService = deductionsService ?? throw new ArgumentNullException(nameof(deductionsService));
            _employeeDeductionsService = employeeDeductionsService;
            _incomeTaxService = incomeTaxService;
            _benefitsService = benefitsService ?? throw new ArgumentNullException(nameof(benefitsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<decimal> CalculateDeductionsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            _logger.LogDebug("=== CALCULANDO DEDUCCIONES ===");
            _logger.LogDebug("Empleado: {Nombre}, Salario: {Salario}",
                empleado.NombreEmpleado, empleado.SalarioBruto);

            try
            {
                var deducciones = _employeeDeductionsService.CalculateEmployeeDeductions(empleado, payrollId, companyId);

                _logger.LogDebug("DEDUCCIONES CALCULADAS: {Deducciones} para {Nombre}",
                     deducciones, empleado.NombreEmpleado);

                return await Task.FromResult(deducciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando deducciones");
                return 0;
            }
        }

        public async Task<decimal> CalculateBenefitsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            try
            {
                _logger.LogDebug("=== INICIANDO CÁLCULO DE BENEFICIOS ===");
                _logger.LogDebug("Empleado: {Nombre} ({Cedula})",
                    empleado.NombreEmpleado, empleado.CedulaEmpleado);

                var totalBenefits = await _benefitsService.CalculateBenefitsAsync(empleado, payrollId, companyId);

                _logger.LogDebug("Total beneficios para {Nombre}: ¢{Total}",
                    empleado.NombreEmpleado, totalBenefits);

                return totalBenefits;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR calculando beneficios para empleado {Cedula}",
                    empleado.CedulaEmpleado);
                return 0m;
            }
        }

        public async Task<decimal> CalculateIncomeTaxAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            _logger.LogDebug("=== CALCULANDO IMPUESTO RENTA ===");
            _logger.LogDebug("Empleado: {Cedula}, Salario: {Salario}",
                empleado.CedulaEmpleado, empleado.SalarioBruto);

            try
            {
                var incomeTax = await Task.Run(() =>
                    _incomeTaxService.CalculateIncomeTax(empleado.SalarioBruto));

                _logger.LogDebug("IMPUESTO RENTA CALCULADO: {IncomeTax} para {Nombre}",
                    incomeTax, empleado.NombreEmpleado);

                return incomeTax;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error calculando impuesto renta");
                throw;
            }
        }


        public async Task<decimal> CalculateEmployerDeductionsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            return await Task.Run(() =>
                _deductionsService.CalculateEmployerDeductions(empleado, payrollId, companyId));
        }
    }
}