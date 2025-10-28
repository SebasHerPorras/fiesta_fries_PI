using backend.Models;
using System;

namespace backend.Interfaces
{
    public interface IEmployeeWorkDayService
    {
        public EmployeeWorkDayModel? GetWorkDay(DateTime weekDate_, DateTime dayDate_,int idEmployee);

        public EmployeeWorkDayModel? AddHours(DateTime dateW,DateTime dateD,int hours, int idEmployee);
    }
}
