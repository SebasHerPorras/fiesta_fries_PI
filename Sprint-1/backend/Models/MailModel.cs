namespace backend.Models
{
    public class MailModel
    {
        public Guid userID { get; set; }
        public string token { get; set; }

        public DateTime experationDate { get; set; }

    }
}
