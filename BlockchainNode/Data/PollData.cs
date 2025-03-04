namespace MainBlockchain
{
    public class PollData
    {
        public bool IsOwner { get; set; }
        public bool IsFinished { get; set; }
        public string PollId { get; set; }
        public string Title { get; set; }
        public PollOption[] Options { get; set; }
        public Dictionary<string, uint> Votes { get; set; }
    }
}