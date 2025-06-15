namespace MainBlockchain
{
    public class FinishRegistration : Transaction
    {
        public string PollId { get; set; }
        protected override string CombineData() => PollId.ToString();
    }
}
