namespace MainBlockchain
{
    public interface IBalanceAffectingTransaction
    {
        public string FromAddress { get; }
        public string ToAddress { get; }
        public decimal Amount { get; }
    }
}