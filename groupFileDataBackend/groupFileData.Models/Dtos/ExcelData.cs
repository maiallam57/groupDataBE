using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace groupFileData.Models.Dtos
{
    public class ExcelData
    {
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

}
