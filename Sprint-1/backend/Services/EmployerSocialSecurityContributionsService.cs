using backend.Models;
using backend.Repositories;
using backend.Interfaces;

namespace backend.Services
{
    public class EmployerSocialSecurityContributionsService : IEmployerSocialSecurityContributionsService
    {
        private readonly EmployerSocialSecurityContributionsRepository _repository;

        public EmployerSocialSecurityContributionsService()
        {
            _repository = new EmployerSocialSecurityContributionsRepository();
        }

        public List<EmployerSocialSecurityContributions> GetActiveContributions()
        {
            try
            {
                return _repository.GetActiveContributions();
            }
            catch (Exception ex)
            {
                return new List<EmployerSocialSecurityContributions>();
            }
        }

        public List<EmployerSocialSecurityContributions> GetAllContributions()
        {
            try
            {
                return _repository.GetAll();
            }
            catch (Exception ex)
            {
                return new List<EmployerSocialSecurityContributions>();
            }
        }
    }
}