using backend.Models;
using backend.Interfaces;
using backend.Interfaces.Services;

namespace backend.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ICalculatorDeductionsEmployerService _deductionsService;
        private readonly ILogger<CalculationService> _logger;

        public CalculationService(
            ICalculatorDeductionsEmployerService deductionsService,
            ILogger<CalculationService> logger)
        {
            _deductionsService = deductionsService ?? throw new ArgumentNullException(nameof(deductionsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<decimal> CalculateDeductionsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            _logger.LogDebug(
                "Calculando deducciones para empleado: {Nombre}, Cédula: {Cedula}, Salario: {Salario}",
                empleado.NombreEmpleado, empleado.CedulaEmpleado, empleado.SalarioBruto);

            try
            {
                var deducciones = _deductionsService.CalculateEmployerDeductions(empleado, payrollId, companyId);

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
            _logger.LogDebug("Calculando beneficios para empleado: {Cedula}", empleado.CedulaEmpleado);

            // implementar servicio real de beneficios
            // por ahora retorna 0 
            await Task.Delay(1); 

            _logger.LogDebug("Beneficios calculados: 0 para empleado {Cedula}", empleado.CedulaEmpleado);
            return 0m;
        }

        public async Task<decimal> CalculateIncomeTaxAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            _logger.LogDebug("Calculando impuesto para empleado: {Cedula}", empleado.CedulaEmpleado);

            // implementar servicio real de impuestos  
            // por ahora retorna 0
            await Task.Delay(1); 

            _logger.LogDebug("Impuesto calculado: 0 para empleado {Cedula}", empleado.CedulaEmpleado);
            return 0m;
        }
    }
}