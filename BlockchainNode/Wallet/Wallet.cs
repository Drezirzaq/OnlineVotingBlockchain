namespace MainBlockchain
{
    public class Wallet
    {
        private readonly Dictionary<string, decimal> _balances;
        private readonly Blockchain _blockchain;
        public Wallet(Blockchain blockchain)
        {
            _balances = new();
            _blockchain = blockchain;
        }
        public bool TryCreateWallet(string address, decimal startAmount)
        {
            if (IsFirstTransaction(address) == false)
            {
                return false;
            }
            var systemTransaction = SystemTransactionFactory.Create(address, startAmount);
            if (_blockchain.TryAddPendingTransaction(systemTransaction) == false)
            {
                return false;
            }
            Console.WriteLine("Кошелек создан");
            return true;
        }

        public decimal GetBalance(string address)
        {
            decimal balance = 0;

            foreach (var block in _blockchain.Chain)
            {
                foreach (var transaction in block.Transactions.OfType<IBalanceAffectingTransaction>())
                {
                    if (transaction.ToAddress == address)
                    {
                        balance += transaction.Amount;
                    }
                    if (transaction.FromAddress == address)
                    {
                        balance -= transaction.Amount;
                    }
                }
            }
            return balance;
        }
        public bool IsFirstTransaction(string address)
        {
            foreach (var block in _blockchain.Chain)
            {
                foreach (var transaction in block.Transactions.OfType<IBalanceAffectingTransaction>())
                {
                    if (transaction.ToAddress == address || transaction.FromAddress == address)
                        return false;
                }
            }
            return true;
        }
    }
}