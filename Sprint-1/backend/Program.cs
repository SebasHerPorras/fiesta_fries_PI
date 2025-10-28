using backend.Interfaces;
using backend.Repositories;
using backend.Services;

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
});

// ===== REGISTRO DE DEPENDENCIAS =====
// Registrar interfaces y sus implementaciones
builder.Services.AddScoped<IEmployerSocialSecurityContributionsService, EmployerSocialSecurityContributionsService>();
builder.Services.AddScoped<IEmployerSocialSecurityByPayrollService, EmployerSocialSecurityByPayrollService>();
builder.Services.AddScoped<ICalculatorDeductionsEmployerService, CalculatorDeductionsEmployerService>();
builder.Services.AddScoped<IEmployeeBenefitService, EmployeeBenefitService>();
builder.Services.AddScoped<EmployeeBenefitRepository>();
builder.Services.AddScoped<IEmployeeSocialSecurityContributionsService, EmployeeSocialSecurityContributionsService>();
builder.Services.AddScoped<IPersonalIncomeTaxService, PersonalIncomeTaxService>();
builder.Services.AddScoped<IEmployeeDeductionsByPayrollService, EmployeeDeductionsByPayrollService>();
builder.Services.AddScoped<ICalculatorDeductionsEmployeeService, CalculatorDeductionsEmployeeService>();

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