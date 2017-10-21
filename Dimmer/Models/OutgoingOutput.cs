using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimmer.Models
{
    public class OutgoingOutput
    {
        public OutgoingOutput(OutgoingInput input)
        {
            this.InterestRate = input.InterestRate;
            this.BankName = input.Bank;
        }

        public double InterestRate { get; set; }

        public string BankName { get; set; }
    }
}
