namespace MainBlockchain
{
    public class FinishPollValidator : IValidator
    {
        private readonly Blockchain _blockchain;
        private readonly FinishPollTransaction _transaction;
        public FinishPollValidator(IValidatable validatable, Blockchain blockchain)
        {
            _blockchain = blockchain;
            if (validatable is FinishPollTransaction transaction)
            {
                _transaction = transaction;
                return;
            }
            throw new System.Exception("Создан неверный валидатор для IValidatable");
        }
        public virtual bool Validate()
        {
            bool opened = false;
            foreach (var block in _blockchain.Chain)
            {
                var openedTransaction = block.Transaction is CreatePollTransaction pollTransaction
                    && pollTransaction.TransactionId.Equals(_transaction.PollId)
                    && pollTransaction.FromAddress.Equals(_transaction.FromAddress);
                if (openedTransaction)
                {
                    opened = true;
                    break;
                }
            }
            if (!opened)
            {
                Console.WriteLine($"Голосования {_transaction.PollId} не существует или голосование начал другой пользователь.");
                return false;
            }
            bool closed = false;
            foreach (var block in _blockchain.Chain)
            {
                var openedTransaction = block.Transaction is FinishPollTransaction pollTransaction
                    && pollTransaction.TransactionId.Equals(_transaction.TransactionId);
                if (openedTransaction)
                {
                    closed = true;
                    break;
                }
            }
            if (closed)
            {
                Console.WriteLine($"Голосование {_transaction.PollId} уже завершено.");
                return false;
            }
            return true;
        }
    }
}