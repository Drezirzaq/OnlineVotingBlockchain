namespace MainBlockchain
{
    public class GetVoteSignatureTransaction : Transaction
    {
        public string PollId { get; set; }
        public string BlindedMessage { get; set; }
        protected override string CombineData() => $"{BlindedMessage}{PollId}";
    }
}