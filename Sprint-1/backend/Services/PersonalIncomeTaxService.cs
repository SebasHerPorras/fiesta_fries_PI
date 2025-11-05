using backend.Models;
using backend.Repositories;
using backend.Interfaces;

namespace backend.Services
{
    public class PersonalIncomeTaxService : IPersonalIncomeTaxService
    {
        private readonly PersonalIncomeTaxRepository _repository;

        public PersonalIncomeTaxService()
        {
            _repository = new PersonalIncomeTaxRepository();
        }

        public List<PersonalIncomeTax> GetActiveScales()
        {
            try
            {
                return _repository.GetActiveScales();
            }
            catch (Exception ex)
            {
                return new List<PersonalIncomeTax>();
            }
        }

        public List<PersonalIncomeTax> GetAllScales()
        {
            try
            {
                return _repository.GetAll();
            }
            catch (Exception ex)
            {
                return new List<PersonalIncomeTax>();
            }
        }

        public decimal CalculateIncomeTax(decimal grossSalary)
        {
            try
            {
                var scales = GetActiveScales();
                
                if (scales == null || !scales.Any())
                    return 0;

                // Encontrar la escala correcta
                var scale = scales
                    .Where(s => grossSalary >= s.MinAmount && (s.MaxAmount == null || grossSalary <= s.MaxAmount))
                    .FirstOrDefault();

                if (scale == null || scale.Percentage == 0)
                    return 0;

                // Calcular impuesto: (salario - monto_minimo) * porcentaje + base_amount
                var taxableAmount = grossSalary - scale.MinAmount;
                var calculatedTax = (taxableAmount * scale.Percentage) + scale.BaseAmount;

                return Math.Round(calculatedTax, 2);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }
}