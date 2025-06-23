namespace MainBlockchain
{
    public class VoteTransactionValidator : IValidator
    {
        private readonly Blockchain _blockchain;
        private readonly VoteTransaction _transaction;
        public VoteTransactionValidator(IValidatable validatable, Blockchain blockchain)
        {
            _blockchain = blockchain;
            if (validatable is VoteTransaction transaction)
            {
                _transaction = transaction;
                return;
            }
            throw new System.Exception("Создан неверный валидатор для IValidatable");
        }
        public virtual bool Validate()
        {
            foreach (var block in _blockchain.Chain)
            {
                var voteExists = block.Transaction is VoteTransaction voteTransaction
                    && voteTransaction.FromAddress.Equals(_transaction.FromAddress)
                    && voteTransaction.PollId.Equals(_transaction.PollId);
                if (voteExists)
                {
                    Console.WriteLine("Попытка проголосовать дважды.");
                    return false;
                }
            }
            return true;
        }
    }
}