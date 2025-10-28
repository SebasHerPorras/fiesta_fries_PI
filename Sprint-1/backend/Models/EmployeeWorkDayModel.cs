namespace backend.Models
{
    public class EmployeeWorkDayModel
    {
        public DateTime date { get; set; }

        public int hours_count { get; set;}

        public DateTime week_start_date { get; set; }

        public int id_employee { get; set; }
    }
}
