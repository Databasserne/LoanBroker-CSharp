﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TranslatorBankSOAP.Models
{
    public class Output
    {
        public Output(Input input, double intrestRate)
        {
            this.SSN = input.SSN;
            this.IntrestRate = intrestRate;
        }

        // ReSharper disable once InconsistentNaming
        [JsonProperty("ssn")]
        public string SSN { get; set; }

        [JsonProperty("interestRate")]
        public double IntrestRate { get; set; }

        public override string ToString()
        {
            return $"SSN: {SSN} - Rate: {IntrestRate}";
        }
    }
}
