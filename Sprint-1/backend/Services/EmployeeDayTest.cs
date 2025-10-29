using backend.Interfaces;
using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public class EmployeeDayTest : IEmployeeWorkDayService
    {
        private readonly List<EmployeeWorkDayModel> _workDays = new();

        public EmployeeWorkDayModel? GetWorkDay(DateTime weekDate_, DateTime dayDate_, int idEmployee)
        {
            return _workDays.FirstOrDefault(w =>
                w.week_start_date == weekDate_ &&
                w.date == dayDate_ &&
                w.id_employee == idEmployee);
        }

        public EmployeeWorkDayModel? AddHours(DateTime dateW, DateTime dateD, int hours, int idEmployee)
        {
            var existing = GetWorkDay(dateW, dateD, idEmployee);

            if (existing == null)
            {
                var newWorkDay = new EmployeeWorkDayModel
                {
                    week_start_date = dateW,
                    date = dateD,
                    id_employee = idEmployee,
                    hours_count = hours
                };

                _workDays.Add(newWorkDay);
                return newWorkDay;
            }

            existing.hours_count += hours;
            return existing;
        }
    }
}
