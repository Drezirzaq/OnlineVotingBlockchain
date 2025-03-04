namespace MainBlockchain
{
    public class FinishPollTransaction : Transaction
    {
        public string PollId { get; set; }
        protected override string CombineData() => PollId;
    }
}