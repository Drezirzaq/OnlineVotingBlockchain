namespace MainBlockchain
{
    public class TransferTransactionFormatValidator : IValidator
    {
        private readonly TransferTransaction _transaction;
        public TransferTransactionFormatValidator(IValidatable validatable)
        {
            if (validatable is TransferTransaction transaction)
            {
                _transaction = transaction;
                return;
            }
            throw new System.Exception("Создан неверный валидатор для IValidatable");
        }
        public virtual bool Validate()
        {
            if (string.IsNullOrEmpty(_transaction.PublicKey) || string.IsNullOrEmpty(_transaction.Signature)
                || string.IsNullOrEmpty(_transaction.FromAddress) || _transaction.Timestamp == default
                || string.IsNullOrEmpty(_transaction.ToAddress) || _transaction.Amount <= 0)
            {
                Console.WriteLine("Ошибка: Неверный формат транзакции.");
                return false;
            }
            return true;
        }
    }
}