namespace MainBlockchain
{
    public class TransactionFormatValidator : IValidator
    {
        private readonly Transaction _transaction;
        public TransactionFormatValidator(IValidatable validatable)
        {
            if (validatable is Transaction transaction)
            {
                _transaction = transaction;
                return;
            }
            throw new System.Exception("Создан неверный валидатор для IValidatable");
        }
        public virtual bool Validate()
        {
            if (string.IsNullOrEmpty(_transaction.PublicKey) || string.IsNullOrEmpty(_transaction.Signature)
                || string.IsNullOrEmpty(_transaction.FromAddress) || _transaction.Timestamp == default)
            {
                Console.WriteLine("Ошибка: Неверный формат транзакции.");
                return false;
            }
            return true;
        }
    }
}