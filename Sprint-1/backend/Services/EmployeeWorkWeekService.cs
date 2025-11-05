using backend.Interfaces;
using backend.Models;
using backend.Repositories;

namespace backend.Services
{
    public class EmployeeWorkWeekService : IEmployeeWorkWeekService
    {
        private readonly EmpleadoRepository _seriviceRepository;    
        public EmployeeWorkWeekService() { 
        
          this._seriviceRepository = new EmpleadoRepository();
        }

        public WeekEmployeeModel? GetWeek(DateTime date_, int idEmployee)
        {
            return this._seriviceRepository.GetWorkWeek(date_, idEmployee);
        }
    }
}
