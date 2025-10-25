using backend.Models;
using backend.Repositories;
using backend.Interfaces;

namespace backend.Services
{
    public class EmployeeSocialSecurityContributionsService : IEmployeeSocialSecurityContributionsService
    {
        private readonly EmployeeSocialSecurityContributionsRepository _repository;

        public EmployeeSocialSecurityContributionsService()
        {
            _repository = new EmployeeSocialSecurityContributionsRepository();
        }

        public List<EmployeeSocialSecurityContributions> GetActiveContributions()
        {
            try
            {
                return _repository.GetActiveContributions();
            }
            catch (Exception ex)
            {
                return new List<EmployeeSocialSecurityContributions>();
            }
        }

        public List<EmployeeSocialSecurityContributions> GetAllContributions()
        {
            try
            {
                return _repository.GetAll();
            }
            catch (Exception ex)
            {
                return new List<EmployeeSocialSecurityContributions>();
            }
        }
    }
}