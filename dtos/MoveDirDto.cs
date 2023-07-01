using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dir2dir.dtos
{
    public class MoveDirDto
    {
        [JsonProperty("froms")]
        public string[] Froms { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }
    }
}
