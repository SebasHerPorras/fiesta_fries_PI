namespace backend.Models
{
    public class PersonalProfileDto
    {
        // De persona
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public DateTime Birthdate { get; set; }
        public string Direction { get; set; }
        public string PersonalPhone { get; set; }
        public string? HomePhone { get; set; }
        // De user
        public string Email { get; set; }

        public string PersonType { get; set; }
    }
}