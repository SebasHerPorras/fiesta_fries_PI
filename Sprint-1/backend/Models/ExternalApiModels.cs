namespace backend.Models
{
    // Response common para todas las APIs
    public class ExternalApiResponse
    {
        public List<ApiDeduction> Deductions { get; set; } = new();
    }

    public class ApiDeduction
    {
        public string Type { get; set; } = string.Empty; // "ER" = Employer, "EE" = Employee
        public decimal Amount { get; set; }
    }

    public class SolidarityAssociationRequest
    {
        public string CompanyLegalId { get; set; } = string.Empty;
        public decimal GrossSalary { get; set; }
    }

    public class PrivateInsuranceRequest
    {
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public int DependentsCount { get; set; }
    }

    public class VoluntaryPensionsRequest
    {
        public string PlanType { get; set; } = string.Empty; // "A", "B", "C"
        public decimal GrossSalary { get; set; }
    }

    public class EmployeeBenefitsDto
    {
        public bool HasSolidarityAssociation { get; set; }
        public bool HasPrivateInsurance { get; set; }
        public bool HasVoluntaryPensions { get; set; }
        
        // Para Seguro Privado
        public DateTime BirthDate { get; set; }
        public int DependentsCount { get; set; }
        
        // Para Pensiones Voluntarias
        public string PensionPlanType { get; set; } = "A"; // A, B, C
    }

    // LEGACY MODELS (Para backward compatibility, deprecated)
    [Obsolete("Use SolidarityAssociationRequest instead")]
    public class AsociacionSolidaristaRequest
    {
        public string CedulaEmpresa { get; set; } = string.Empty;
        public decimal SalarioBruto { get; set; }
    }

    [Obsolete("Use PrivateInsuranceRequest instead")]
    public class SeguroPrivadoRequest
    {
        public int Edad { get; set; }
        public DateTime fechaDeNacimiento { get; set; }
        public int CantidadDependientes { get; set; }
    }

    [Obsolete("Use VoluntaryPensionsRequest instead")]
    public class PensionesVoluntariasRequest
    {
        public string Tipo { get; set; } = string.Empty; // "A", "B", "C"
        public decimal SalarioBruto { get; set; }
    }
}