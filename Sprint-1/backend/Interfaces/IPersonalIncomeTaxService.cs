using backend.Models;

namespace backend.Interfaces
{
    public interface IPersonalIncomeTaxService
    {
        List<PersonalIncomeTax> GetActiveScales();
        List<PersonalIncomeTax> GetAllScales();
        decimal CalculateIncomeTax(decimal grossSalary);
    }
}