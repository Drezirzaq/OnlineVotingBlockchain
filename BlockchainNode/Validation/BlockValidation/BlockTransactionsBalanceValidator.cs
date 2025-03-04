namespace MainBlockchain
{
    public class BlockTransactionsBalanceValidator : IValidator
    {
        private readonly Block _block;
        private readonly Wallet _wallet;

        public BlockTransactionsBalanceValidator(IValidatable validatable, Wallet wallet)
        {
            _wallet = wallet;
            if (validatable is Block block)
            {
                _block = block;
                return;
            }
            throw new System.Exception("Wrong vaidator for block created");
        }
        public bool Validate()
        {
            Dictionary<string, decimal> tempBalances = new Dictionary<string, decimal>();

            foreach (var transaction in _block.Transactions.OfType<IBalanceAffectingTransaction>())
            {
                if (!tempBalances.ContainsKey(transaction.FromAddress))
                {
                    tempBalances[transaction.FromAddress] = _wallet.GetBalance(transaction.FromAddress);
                }

                tempBalances[transaction.FromAddress] -= transaction.Amount;

                if (tempBalances.ContainsKey(transaction.ToAddress))
                {
                    tempBalances[transaction.ToAddress] += transaction.Amount;
                }
            }
            tempBalances.Remove("system");
            return tempBalances.All(x => x.Value >= 0);
        }
    }
}