using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployeeSocialSecurityContributionsService
    {
        List<EmployeeSocialSecurityContributions> GetActiveContributions();
        List<EmployeeSocialSecurityContributions> GetAllContributions();
    }
}