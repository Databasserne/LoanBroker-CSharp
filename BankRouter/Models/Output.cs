namespace BankRouter.Models
{
    public class Output
    {
        public Output(Input input)
        {
            SSN = input.SSN;
            Months = input.Months;
            Amount = input.Amount;
            CreditScore = input.CreditScore;
        }

        // ReSharper disable once InconsistentNaming
        public string SSN { get; set; }

        public int Months { get; set; }

        public double Amount { get; set; }

        public int CreditScore { get; set; }

        public override string ToString()
        {
            return $"SSN: {SSN} - Months: {Months} - Amount: {Amount} - CreditScore: {CreditScore}";
        }
    }
}
