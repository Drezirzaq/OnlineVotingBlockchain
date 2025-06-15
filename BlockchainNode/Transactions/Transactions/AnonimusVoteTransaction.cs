using BlockchainNode.Data;

namespace MainBlockchain
{
    public class AnonimusVoteTransaction : Transaction
    {
        public VotePayload VotePayload { get; set; }
        protected override string CombineData() => string.Empty;
    }
}
