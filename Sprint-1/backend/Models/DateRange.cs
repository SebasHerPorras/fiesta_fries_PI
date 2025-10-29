namespace backend.Models.Common
{
    public record DateRange(DateTime Start, DateTime End)
    {
        public bool Contains(DateTime date) => date >= Start && date <= End;
        public TimeSpan Duration => End - Start;
    }
}