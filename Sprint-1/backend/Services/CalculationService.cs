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
        private readonly IEmployeeSocialSecurityContributionsService _socialSecurityService;
        private readonly IEmployeeDeductionsByPayrollService _payrollDeductionService;
        private readonly IEmployerBenefitDeductionService _employerBenefitService;
        private readonly IExternalApiFactory _apiFactory;
        private readonly IEmployeeBenefitService _employeeBenefitService;
        private readonly IEmployerSocialSecurityContributionsService _employerSocialSecurityService;
        private readonly IEmployerSocialSecurityByPayrollService _employerPayrollService;

        public CalculationService(
            ICalculatorDeductionsEmployerService deductionsService,
            IPersonalIncomeTaxService incomeTaxService,
            ILogger<CalculationService> logger,
            IServiceProvider serviceProvider,
            IEmployeeSocialSecurityContributionsService socialSecurityService,
            IEmployeeDeductionsByPayrollService payrollDeductionService,
            IEmployerBenefitDeductionService employerBenefitService,
            IExternalApiFactory apiFactory,
            IEmployeeBenefitService employeeBenefitService,
            IEmployerSocialSecurityContributionsService employerSocialSecurityService,
            IEmployerSocialSecurityByPayrollService employerPayrollService)
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
            _employerSocialSecurityService = employerSocialSecurityService;
            _employerPayrollService = employerPayrollService;
        }

        public async Task<decimal> CalculateDeductionsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            _logger.LogInformation("🔵 === CALCULANDO DEDUCCIONES EMPLEADO ===");
            _logger.LogInformation("🔵 Empleado: {Nombre}, Salario: ₡{Salario}, PayrollId: {PayrollId}",
                empleado.NombreEmpleado, empleado.SalarioBruto, payrollId);

            try
            {
                bool saveInDB = payrollId > 0;
                _logger.LogInformation("🔵 SaveInDB: {SaveInDB}", saveInDB);

                var employeeDeductionsService = new CalculatorDeductionsEmployeeService(
                    _socialSecurityService,
                    _incomeTaxService,
                    _payrollDeductionService,
                    saveInDB
                );

                var deducciones = employeeDeductionsService.CalculateEmployeeDeductions(empleado, payrollId, companyId);

                _logger.LogInformation("🔵 DEDUCCIONES CALCULADAS: ₡{Deducciones} para {Nombre}",
                     deducciones, empleado.NombreEmpleado);

                return await Task.FromResult(deducciones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR calculando deducciones - RETORNANDO 0");
                return 0;
            }
        }

        public async Task<decimal> CalculateBenefitsAsync(EmployeeCalculationDto empleado, long companyId, int payrollId)
        {
            try
            {
                _logger.LogInformation("🟢 === CALCULANDO BENEFICIOS ===");
                _logger.LogInformation("🟢 Empleado: {Nombre} ({Cedula}), PayrollId: {PayrollId}",
                    empleado.NombreEmpleado, empleado.CedulaEmpleado, payrollId);

                bool saveInDB = payrollId > 0;
                _logger.LogInformation("🟢 SaveInDB: {SaveInDB}", saveInDB);

                var benefitsService = new CalculatorBenefitsService(
                    _payrollDeductionService,
                    _employerBenefitService,
                    _apiFactory,
                    _employeeBenefitService,
                    _serviceProvider.GetRequiredService<ILogger<CalculatorBenefitsService>>(),
                    saveInDB
                );

                var totalBenefits = await benefitsService.CalculateBenefitsAsync(empleado, payrollId, companyId);

                _logger.LogInformation("🟢 BENEFICIOS CALCULADOS: ₡{Total} para {Nombre}",
                    totalBenefits, empleado.NombreEmpleado);

                return totalBenefits;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR calculando beneficios para empleado {Cedula} - RETORNANDO 0",
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
            _logger.LogInformation("🟡 === CALCULANDO CARGAS SOCIALES PATRONALES ===");
            _logger.LogInformation("🟡 Empleado: {Nombre}, Salario: ₡{Salario}, PayrollId: {PayrollId}",
                empleado.NombreEmpleado, empleado.SalarioBruto, payrollId);

            try
            {
                bool saveInDB = payrollId > 0;
                _logger.LogInformation("🟡 SaveInDB: {SaveInDB}", saveInDB);
                
                var employerDeductionsService = new CalculatorDeductionsEmployerService(
                    _employerSocialSecurityService,
                    _employerPayrollService,
                    saveInDB
                );

                var deductions = employerDeductionsService
                    .CalculateEmployerDeductions(empleado, payrollId, companyId);

                _logger.LogInformation("🟡 CARGAS PATRONALES CALCULADAS: ₡{Deductions} para {Nombre}",
                     deductions, empleado.NombreEmpleado);

                return deductions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR calculando cargas patronales - RETORNANDO 0");
                return 0;
            }
        }
    }
}