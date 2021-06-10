namespace System.Process.Domain.ValueObjects
{
    public class Activity
    {
        public string Type { get; set; }
        public string Frequency { get; set; }
        public decimal? Percentage { get; set; }
    }
}