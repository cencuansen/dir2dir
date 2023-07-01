using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dir2dir.dtos
{
    public class MessageDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("data")]
        public dynamic Data { get; set; }
    }
}
