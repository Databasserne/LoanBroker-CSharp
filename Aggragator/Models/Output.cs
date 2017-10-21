using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Aggragator.Models
{
    public class Output
    {
        public Output(Input input)
        {
            this.SSN = input.SSN;
            this.InterestRate = input.InterestRate;
            this.Bank = input.Bank;
        }

        // ReSharper disable once InconsistentNaming
        public string SSN { get; set; }

        public double InterestRate { get; set; }

        public string Bank { get; set; }

        public override string ToString()
        {
            return $"SSN: {SSN} - Rate: {InterestRate} - Bank: {Bank}";
        }
    }
}
