namespace backend.Models
  {
    public class EmpleadoUpdateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string SecondName { get; set; } = string.Empty;
        public string Direction { get; set; } = string.Empty;
        public string PersonalPhone { get; set; } = string.Empty;
        public string? HomePhone { get; set; }
        public string Position { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int Salary { get; set; }
    }

}