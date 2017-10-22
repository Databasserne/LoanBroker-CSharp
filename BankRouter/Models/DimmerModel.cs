using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankRouter.Models
{
    public class DimmerModel
    {
        public DimmerModel(Input input)
        {
            SSN = input.SSN;
            InterestRate = -1;
            Bank = null;
        }

        // ReSharper disable once InconsistentNaming
        public string SSN { get; set; }

        public double InterestRate { get; set; }

        public string Bank { get; set; }
    }
}
