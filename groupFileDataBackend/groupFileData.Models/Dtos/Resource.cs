using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace groupFileData.Models.Dtos
{
    
    public class Resource
    {
        public string ServiceDate { get; set; }
        public string PONumber { get; set; }
        public string Account { get; set; }
        public decimal BillingRate { get; set; }
        public decimal TotalGross { get; set; }
        public decimal TotalGrossUsd { get; set; }
    }
}
