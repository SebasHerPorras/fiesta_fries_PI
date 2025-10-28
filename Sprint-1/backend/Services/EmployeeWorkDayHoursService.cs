using backend.Interfaces;
using backend.Models;
using backend.Repositories;
using System;

namespace backend.Services
{
    public class EmployeeWorkDayHoursService: IEmployeeWorkDayService
    {
        private readonly EmpleadoRepository _seriviceRepository;
        public EmployeeWorkDayHoursService()
        {
            this._seriviceRepository = new EmpleadoRepository();
        }

        public EmployeeWorkDayModel? GetWorkDay(DateTime weekDate_, DateTime dayDate_, int idEmployee)
        {
            return this._seriviceRepository.GetWorkDay(weekDate_,dayDate_,idEmployee);

        }

        public EmployeeWorkDayModel? AddHours(DateTime dateW,DateTime dateD, int hours, int idEmployee)
        {
            return this._seriviceRepository.AddHours(dateW,dateD ,hours, idEmployee);
        }
    }
}
