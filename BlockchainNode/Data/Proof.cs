using System.Text.Json.Serialization;

namespace BlockchainNode.Data
{
    public class Proof
    {
        [JsonPropertyName("pi_a")]
        public string[] PiA { get; set; }

        [JsonPropertyName("pi_b")]
        public string[][] PiB { get; set; }
        [JsonPropertyName("pi_c")]
        public string[] PiC { get; set; }

        public string Protocol { get; set; }
        public string Curve { get; set; } 

    }
}
