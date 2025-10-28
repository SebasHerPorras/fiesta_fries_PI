using backend.Models;

namespace backend.Interfaces
{
    public interface IEmployeeWorkWeekService
    {
        public WeekEmployeeModel? GetWeek(DateTime date_, int idEmployee);

    }
}
