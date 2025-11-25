using backend.Models.Payroll;
using System.Threading.Tasks;

namespace backend.Interfaces.Repositories
{
    public interface IPayrollPositionRepository
    {
        Task SavePositionAsync(PayrollPosition position);
    }
}
