using backend.Handlers.backend.Repositories;
using backend.Infrastructure;
using backend.Interfaces;
using backend.Interfaces.Services;
using backend.Interfaces.Strategies;
using backend.Repositories;
using backend.Services;
using backend.Services.Strategies;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:8080")
                                .AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials();
                      });
});

// Añadir controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Fiesta Fries API", 
        Version = "v1",
        Description = "API para el cálculo de deducciones patronales y gestión de empleados"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
});

// ===== REGISTRO DE DEPENDENCIAS =====
// Registrar interfaces y sus implementaciones
builder.Services.AddScoped<IEmployerSocialSecurityContributionsService, EmployerSocialSecurityContributionsService>();
builder.Services.AddScoped<IEmployerSocialSecurityByPayrollService, EmployerSocialSecurityByPayrollService>();
builder.Services.AddScoped<ICalculatorDeductionsEmployerService, CalculatorDeductionsEmployerService>();
builder.Services.AddScoped<IEmployeeBenefitService, EmployeeBenefitService>();
builder.Services.AddScoped<IEmployeeBenefitRepository, EmployeeBenefitRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IEmployeeSocialSecurityContributionsService, EmployeeSocialSecurityContributionsService>();
builder.Services.AddScoped<IPersonalIncomeTaxService, PersonalIncomeTaxService>();
builder.Services.AddScoped<IEmployeeDeductionsByPayrollService, EmployeeDeductionsByPayrollService>();
builder.Services.AddScoped<ICalculatorDeductionsEmployeeService, CalculatorDeductionsEmployeeService>();
builder.Services.AddScoped<ICalculatorBenefitsService, CalculatorBenefitsService>();
builder.Services.AddScoped<IEmployerBenefitDeductionService, EmployerBenefitDeductionService>();
builder.Services.AddScoped<IPayrollPeriodService, PayrollPeriodService>();
builder.Services.AddScoped<IPeriodCalculator, BiweeklyPeriodCalculator>();
builder.Services.AddScoped<IPeriodCalculator, MonthlyPeriodCalculator>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<ICalculationService, CalculationService>();

// Deletion Service
builder.Services.AddScoped<IEmployeeDeletionRepository, EmployeeDeletionRepository>();
builder.Services.AddScoped<IEmployeeDeletionService, EmployeeDeletionService>();

// ===== CONFIGURACIÓN DE HTTP CLIENTS PARA APIS EXTERNAS =====
builder.Services.AddHttpClient<ISolidarityAssociationService, SolidarityAssociationService>("AsociacionSolidarista", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IPrivateInsuranceService, PrivateInsuranceService>("SeguroPrivado", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IVoluntaryPensionsService, VoluntaryPensionsService>("PensionesVoluntarias", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ===== REGISTRO DE SERVICIOS DE APIS EXTERNAS =====
builder.Services.AddScoped<ISolidarityAssociationService, SolidarityAssociationService>();
builder.Services.AddScoped<IPrivateInsuranceService, PrivateInsuranceService>();
builder.Services.AddScoped<IVoluntaryPensionsService, VoluntaryPensionsService>();
builder.Services.AddScoped<IExternalApiFactory, ExternalApiFactory>();

builder.Services.AddScoped<IPayrollRepository, PayrollRepository>();
builder.Services.AddScoped<ICalculationService, CalculationService>();
builder.Services.AddScoped<IEmployeeService, EmpleadoService>();
builder.Services.AddScoped<IPayrollProcessingService, PayrollProcessingService>();
builder.Services.AddScoped<IPayrollValidator, PayrollValidator>();
builder.Services.AddScoped<IPayrollResultBuilder, PayrollResultBuilder>();
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

// Payroll Reports employee
builder.Services.AddScoped<PayrollReportRepository>();
builder.Services.AddScoped<PayrollPdfService>();
builder.Services.AddScoped<PayrollCsvService>();
builder.Services.AddScoped<PayrollReportService>();

// Beneficios
builder.Services.AddScoped<BeneficioRepository>();
builder.Services.AddScoped<IBeneficioService, BeneficioService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<EmpresaService>();

var app = builder.Build();

// Solo habilitar Swagger en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fiesta Fries API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors(MyAllowSpecificOrigins);
app.UseAuthorization();
app.MapControllers();

app.Urls.Clear();
app.Urls.Add("http://localhost:5081");
app.Urls.Add("https://localhost:7056");

app.Run();