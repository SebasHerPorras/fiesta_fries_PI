namespace backend.Interfaces
{
    public interface ICompanyDeletionService
    {
        Task<CompanyDeletionResult> DeleteCompany(long cedulaJuridica);
    }

    public record CompanyDeletionResult
    {
        public bool Success { get; init; }
        public string Message { get; init; }
        public int EmployeesProcessed { get; init; }
        public int SuccessfulDeletions { get; init; }
        public int BenefitsProcessed { get; init; }
        public int SuccessfulBenefitDeletions { get; init; }
    }
}
