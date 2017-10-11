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
            this.Rate = input.Rate;
        }

        public double Rate { get; set; }
    }
}
