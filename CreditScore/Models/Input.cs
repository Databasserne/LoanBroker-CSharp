namespace CreditScore.Models
{
    public class Input
    {
        // ReSharper disable once InconsistentNaming
        public string SSN { get; set; }

        public int Months { get; set; }

        public double Amount { get; set; }

        public override string ToString()
        {
            return $"SSN: {SSN} - Months: {Months} - Amount: {Amount}";
        }
    }
}
