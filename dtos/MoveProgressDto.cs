using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dir2dir.dtos
{
    public class MoveProgressDto
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("percentage")]
        public double Percentage { get; set; }
    }
}
