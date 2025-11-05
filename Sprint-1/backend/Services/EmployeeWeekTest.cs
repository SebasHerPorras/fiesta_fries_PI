using backend.Interfaces;
using backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace backend.Services
{
    public class EmployeeWeekTest : IEmployeeWorkWeekService
    {
        private readonly List<WeekEmployeeModel> _workWeek;

        public EmployeeWeekTest()
        {
            this._workWeek = new List<WeekEmployeeModel>
            {
                new WeekEmployeeModel
                {
                    start_date = new DateTime(2025, 10, 20),
                    id_employee = 119180741,
                    hours_count = 40
                },
                new WeekEmployeeModel
                {
                    start_date = new DateTime(2025, 10, 27),
                    id_employee = 123456789,
                    hours_count = 35
                },
                new WeekEmployeeModel
                {
                    start_date = new DateTime(2025, 11, 03),
                    id_employee = 987654321,
                    hours_count = 23
                },
                new WeekEmployeeModel
                {
                    start_date = new DateTime(2025, 11, 10),
                    id_employee = 112233445,
                    hours_count = 50
                }
            };
        }

        public WeekEmployeeModel? GetWeek(DateTime date_, int idEmployee)
        {
            return this._workWeek.FirstOrDefault(w =>
                w.start_date == date_ && w.id_employee == idEmployee);
        }
    }
}
