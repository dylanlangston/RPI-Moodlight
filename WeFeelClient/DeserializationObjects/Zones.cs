using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WeFeelClient.DeserializationObjects
{
    public class Zones
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }
    }

    public class ZonesTree : Zones
    {
        [JsonPropertyName("children")]
        public List<Zones> Children { get; set; }
    }
}
