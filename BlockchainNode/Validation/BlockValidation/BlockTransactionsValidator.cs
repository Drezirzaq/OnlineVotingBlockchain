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
            if (ValidationHandler.IsValid(_block.Transaction) == false)
            {
                Console.WriteLine("Найдена невалидная транзакция в блоке.");
                return false;
            }

            foreach (var block in _blockchain.Chain.Take(10))
            {
                if (block.Transaction.TransactionId == _block.Transaction.TransactionId)
                {
                    Console.WriteLine("Транзакция в новом блоке совпадает с транзакцией уже внесенной в блокчейн.");
                    return false;
                }
            }
            return true;
        }
    }
}