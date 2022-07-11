using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WeFeelClient.DeserializationObjects
{
    public class LocalStartClass
    {
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("zone")]
        public string Zone { get; set; }
    }

    public class Timepoints
    {
        [JsonConverter(typeof(DateTimeConverter))]
        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("localStart")]
        public LocalStartClass LocalStart { get; set; }
        
        [JsonPropertyName("earliestLocalStart")]
        public LocalStartClass EarliestLocalStart { get; set; }

        [JsonPropertyName("latestLocalStart")]
        public LocalStartClass LatestLocalStart { get; set; }

        [JsonPropertyName("counts")]
        public Dictionary<string, double> Counts { get; set; }
    }
}
