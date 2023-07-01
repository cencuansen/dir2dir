using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dir2dir.dtos
{
    public class DirSizeDto
    {
        [JsonProperty("fullPath")]
        public string FullPath { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }
}
