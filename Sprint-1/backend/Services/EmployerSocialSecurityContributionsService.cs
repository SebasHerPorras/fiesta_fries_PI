using backend.Models;
using backend.Repositories;

namespace backend.Services
{
    public class EmployerSocialSecurityContributionsService
    {
        private readonly EmployerSocialSecurityContributionsRepository _repository;

        public EmployerSocialSecurityContributionsService()
        {
            _repository = new EmployerSocialSecurityContributionsRepository();
        }

        // solo devuelve las cargas sociales activas
        // NOMBRE y MONTO
        public List<EmployerSocialSecurityContributions> GetActiveContributions()
        {
            try
            {
                return _repository.GetActiveContributions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Service GetActiveContributions: {ex.Message}");
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
                Console.WriteLine($"[ERROR] Service GetAllContributions: {ex.Message}");
                return new List<EmployerSocialSecurityContributions>();
            }
        }
    }
}