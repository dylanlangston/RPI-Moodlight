using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WeFeelClient.DeserializationObjects
{
    public class NormsClass
    {
        [JsonPropertyName("valence")]
        public double Valence { get; set; }

        [JsonPropertyName("arousal")]
        public double Arousal { get; set; }

        [JsonPropertyName("dominance")]
        public double Dominance { get; set; }
    }

    public class PathsClass
    {
        [JsonPropertyName("primaryEmotion")]
        public string PrimaryEmotion { get; set; }

        [JsonPropertyName("secondaryEmotion")]
        public string SecondaryEmotion { get; set; }
    }

    public abstract class Emotions
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("norms")]
        public NormsClass Norms { get; set; }
    }

    public class EmotionsTree : Emotions
    {
        [JsonPropertyName("children")]
        public List<EmotionsTree> Children { get; set; }
    }

    public class EmotionsPrimary : Emotions
    {
        [JsonPropertyName("secondaryEmotions")]
        public List<string> SecondaryEmotions { get; set; }
    }

    public class EmotionsSecondary : Emotions
    {
        [JsonPropertyName("primaryEmotion")]
        public string PrimaryEmotion { get; set; }

        [JsonPropertyName("rawEmotions")]
        public List<string> RawEmotions { get; set; }
    }

    public class EmotionsSecondaryRaw : Emotions
    {
        [JsonPropertyName("paths")]
        public List<PathsClass> Paths { get; set; }
    }
}
