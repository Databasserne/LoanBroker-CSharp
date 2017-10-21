using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Aggragator.Models
{
    public class Input
    {
        [JsonProperty("ssn")]
        // ReSharper disable once InconsistentNaming
        public string SSN { get; set; }

        [JsonProperty("interestRate")]
        public double InterestRate { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        public override string ToString()
        {
            return $"SSN: {SSN} - Rate: {InterestRate} - Bank: {Bank}";
        }
    }
}
