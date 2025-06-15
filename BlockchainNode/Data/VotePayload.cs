using MainBlockchain;
using System.Text.Json.Serialization;

namespace BlockchainNode.Data
{
    public class VotePayload : IValidatable
    {
        public Proof MembershipProof { get; set; }
        public List<string> MembershipSignals { get; set; }
        public Proof VoteProof { get; set; }
        public List<string> VoteSignals { get; set; }
        public string Nullifier { get; set; }
        public string PollId { get; set; } 
        public string WeightHash { get; set; }
        public string Root { get; set; }
        public int OptionId { get; set; }
        public string C1x { get; set; }
        public string C1y { get; set; }
        public string C2x { get; set; }
        public string C2y { get; set; }

        public string ValidationId => "VotePayload";
    }
}
