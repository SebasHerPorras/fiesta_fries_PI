using backend.Interfaces;
using backend.Interfaces.Services;
using backend.Models;
using backend.Repositories;

namespace backend.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly ICalculatorDeductionsEmployerService _deductionsService;
        private readonly IPersonalIncomeTaxService _incomeTaxService;
        private readonly ILogger<CalculationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        // Servicios base para crear instancias con saveInDB
        private readonly IEmployeeSocialSecurityContributionsService _socialSecurityService;
        private readonly IEmployeeDeductionsByPayrollService _payrollDeductionService;
        private readonly IEmployerBenefitDeductionService _employerBenefitService;
        private readonly IExternalApiFactory _apiFactory;
        private readonly IEmployeeBenefitService _employeeBenefitService;

        public CalculationService(
            ICalculatorDeductionsEmployerService deductionsService,
            IPersonalIncomeTaxService incomeTaxService,
            ILogger<CalculationService> logger,
            IServiceProvider serviceProvider,
            IEmployeeSocialSecurityContributionsService socialSecurityService,
            IEmployeeDeductionsByPayrollService payrollDeductionService,
            IEmployerBenefitDeductionService employerBenefitService,
            IExternalApiFactory apiFactory,
            IEmployeeBenefitService employeeBenefitService)
        {
            _deductionsService = deductionsService ?? throw new ArgumentNullException(nameof(deductionsService));
            _incomeTaxService = incomeTaxService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider;
            _socialSecurityService = socialSecurityService;
            _payrollDeductionService = payrollDeductionService;
            _employerBenefitService = employerBenefitService;
            _apiFactory = apiFactory;
            _employeeBenefitService = employeeBenefitService;
        }

        public async Task<decimal> CalculateDeductionsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            _logger.LogDebug("=== CALCULANDO DEDUCCIONES ===");
            _logger.LogDebug("Empleado: {Nombre}, Salario: {Salario}, PayrollId: {PayrollId}",
                empleado.NombreEmpleado, empleado.SalarioBruto, payrollId);

            try
            {
                // Si payrollId == 0, es PREVIEW (no guardar en BD)
                bool saveInDB = payrollId > 0;
                
                var employeeDeductionsService = new CalculatorDeductionsEmployeeService(
                    _socialSecurityService,
                    _incomeTaxService,
                    _payrollDeductionService,
                    saveInDB
                );

                var deducciones = employeeDeductionsService.CalculateEmployeeDeductions(empleado, payrollId, companyId);

                _logger.LogDebug("DEDUCCIONES CALCULADAS: {Deducciones} para {Nombre} (SaveInDB: {SaveInDB})",
                     deducciones, empleado.NombreEmpleado, saveInDB);

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
                _logger.LogDebug("Empleado: {Nombre} ({Cedula}), PayrollId: {PayrollId}",
                    empleado.NombreEmpleado, empleado.CedulaEmpleado, payrollId);

                // Si payrollId == 0, es PREVIEW (no guardar en BD)
                bool saveInDB = payrollId > 0;

                var benefitsService = new CalculatorBenefitsService(
                    _payrollDeductionService,
                    _employerBenefitService,
                    _apiFactory,
                    _employeeBenefitService,
                    _serviceProvider.GetRequiredService<ILogger<CalculatorBenefitsService>>(),
                    saveInDB
                );

                var totalBenefits = await benefitsService.CalculateBenefitsAsync(empleado, payrollId, companyId);

                _logger.LogDebug("Total beneficios para {Nombre}: ₡{Total}",
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