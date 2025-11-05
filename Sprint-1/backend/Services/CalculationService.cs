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
            _logger.LogDebug(
                "Calculando deducciones para empleado: {Nombre}, Cédula: {Cedula}, Salario: {Salario}",
                empleado.NombreEmpleado, empleado.CedulaEmpleado, empleado.SalarioBruto);

            try
            {
                var deducciones = _employeeDeductionsService.CalculateEmployeeDeductions(empleado, payrollId, companyId);

                _logger.LogDebug(
                    "Deducciones calculadas: {Deducciones} para empleado {Cedula}",
                    deducciones, empleado.CedulaEmpleado);

                return await Task.FromResult(deducciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Error calculando deducciones para empleado {Cedula}",
                    empleado.CedulaEmpleado);
                throw;
            }
        }

        public async Task<decimal> CalculateBenefitsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            try
            {
                _logger.LogInformation("=== INICIANDO CÁLCULO DE BENEFICIOS ===");
                _logger.LogInformation("Empleado: {Nombre} ({Cedula})",
                    empleado.NombreEmpleado, empleado.CedulaEmpleado);

                var totalBenefits = await _benefitsService.CalculateBenefitsAsync(empleado, payrollId, companyId);

                _logger.LogInformation("Total beneficios para {Nombre}: ₡{Total}",
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
            _logger.LogInformation("=== CALCULANDO IMPUESTO RENTA ===");
            _logger.LogInformation("Empleado: {Cedula}, Salario: {Salario}",
                empleado.CedulaEmpleado, empleado.SalarioBruto);

            try
            {
                var incomeTax = await Task.Run(() =>
                    _incomeTaxService.CalculateIncomeTax(empleado.SalarioBruto));

                _logger.LogInformation("=== IMPUESTO RENTA CALCULADO: {IncomeTax} ===", incomeTax);

                return incomeTax;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculando impuesto renta para empleado {Cedula}",
                    empleado.CedulaEmpleado);
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