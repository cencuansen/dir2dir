using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dir2dir.dtos
{
    public class DirItemDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("cTime")]
        public DateTime CreatedTime { get; set; }

        [JsonProperty("mTime")]
        public DateTime ModifiedTime { get; set; }

        [JsonProperty("fullPath")]
        public string FullPath { get; set; }

        [JsonProperty("isFile")]
        public bool IsFile { get; set; }

        [JsonProperty("isDir")]
        public bool IsDir { get; set; }

        [JsonProperty("isLink")]
        public bool IsLink { get; set; }

        [JsonProperty("isHidden")]
        public bool IsHidden { get; set; }

        [JsonProperty("isReadOnly")]
        public bool IsReadOnly { get; set; }

    }
}
