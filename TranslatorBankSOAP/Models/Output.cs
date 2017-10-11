using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslatorBankSOAP.Models
{
    public class Output
    {
        public Output(Input input, double rate)
        {
            this.SSN = input.SSN;
            this.Months = input.Months;
            this.Amount = input.Amount;
            this.CreditScore = input.CreditScore;
            this.Rate = rate;
        }

        // ReSharper disable once InconsistentNaming
        public string SSN { get; set; }

        public int Months { get; set; }

        public double Amount { get; set; }

        public int CreditScore { get; set; }

        public double Rate { get; set; }

        public override string ToString()
        {
            return $"SSN: {SSN} - Months: {Months} - Amount: {Amount} - CreditScore: {CreditScore} - Rate: {Rate}";
        }
    }
}
