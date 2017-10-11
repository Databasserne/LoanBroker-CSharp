using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dimmer.Models
{
    public class Ingoing
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
