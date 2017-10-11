using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslatorBankSOAP.Models
{
    public class Input
    {
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
