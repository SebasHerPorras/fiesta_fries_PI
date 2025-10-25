using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployerSocialSecurityContributionsService
    {
        List<EmployerSocialSecurityContributions> GetActiveContributions();
        List<EmployerSocialSecurityContributions> GetAllContributions();
    }
}