namespace MainBlockchain
{
    public class TransferTransaction : Transaction, IBalanceAffectingTransaction
    {
        public string ToAddress { get; set; }
        public virtual decimal Amount { get; set; }

        protected override string CombineData()
        {
            return $"{ToAddress}{Amount}";
        }
    }
}