namespace MainBlockchain
{
    public class BlockTransactionsValidator : IValidator
    {
        private readonly Block _block;
        private readonly Blockchain _blockchain;

        public BlockTransactionsValidator(IValidatable validatable, Blockchain blockchain)
        {
            _blockchain = blockchain;
            if (validatable is Block block)
            {
                _block = block;
                return;
            }
            throw new System.Exception("Wrong vaidator for block created");
        }
        public bool Validate()
        {
            HashSet<string> ids = new();
            foreach (var transaction in _block.Transactions)
            {
                if (ids.Add(transaction.TransactionId) == false)
                {
                    Console.WriteLine("Найдены транзакции с одинаковым id в блоке.");
                    return false;
                }
                if (ValidationHandler.IsValid(transaction) == false)
                {
                    Console.WriteLine("Найдена невалидная транзакция в блоке.");
                    return false;
                }
            }
            var blocks = _blockchain.Chain.Take(10);
            foreach (var block in blocks)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (ids.Add(transaction.TransactionId) == false)
                    {
                        Console.WriteLine("Одна из транзакций совпадает с транзакцией уже внесенной в блокчейн.");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}