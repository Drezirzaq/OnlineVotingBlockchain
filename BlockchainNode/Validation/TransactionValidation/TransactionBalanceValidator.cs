namespace MainBlockchain
{
    public class TransactionBalanceValidator : IValidator
    {
        private readonly IBalanceAffectingTransaction _transaction;
        private readonly Wallet _wallet;
        public TransactionBalanceValidator(IValidatable validatable, Wallet wallet)
        {
            _wallet = wallet;
            if (validatable is IBalanceAffectingTransaction transaction)
            {
                _transaction = transaction;
                return;
            }
            throw new System.Exception("Создан неверный валидатор для IValidatable");
        }
        public virtual bool Validate()
        {
            var balance = _wallet.GetBalance(_transaction.FromAddress);
            if (balance < _transaction.Amount)
            {
                Console.WriteLine("Недостаточно средств для перевода");
                return false;
            }
            return true;
        }
    }
}