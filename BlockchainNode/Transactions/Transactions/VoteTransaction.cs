namespace MainBlockchain
{
    public class VoteTransaction : Transaction
    {
        public string PollId { get; set; }
        public string OptionId { get; set; }
        protected override string CombineData() => $"{PollId}{OptionId}";
    }
}
