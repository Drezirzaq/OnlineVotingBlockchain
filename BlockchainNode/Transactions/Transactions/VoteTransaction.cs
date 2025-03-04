namespace MainBlockchain
{
    public class VoteTransaction : TransferTransaction
    {
        public string PollId { get; set; }
        public override decimal Amount => 1;
        protected override string CombineData() => $"{ToAddress}{PollId}";
    }
}