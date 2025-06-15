namespace MainBlockchain
{
    public class PollData
    {
        public bool IsOwner { get; set; }
        public bool IsFinished { get; set; }
        public string PollId { get; set; }
        public string Title { get; set; }
        public string PublicKey { get; set; }
        public bool IsPrivate { get; set; }
        public bool HasPermission { get; set; }
        public PollOption[] Options { get; set; }
        public Dictionary<string, int> Votes { get; set; }
        public int TokensAvailable { get; set; }
        public int PrivatePollStatus { get; set; }
    }
}