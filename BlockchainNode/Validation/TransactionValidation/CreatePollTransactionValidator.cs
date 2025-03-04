namespace MainBlockchain
{
    public class CreatePollTransactionValidator : IValidator
    {
        private readonly CreatePollTransaction _transaction;

        public CreatePollTransactionValidator(IValidatable validatable)
        {
            if (validatable is CreatePollTransaction transaction)
            {
                _transaction = transaction;
                return;
            }
            throw new System.Exception("Wrong vaidator for block created");
        }

        public bool Validate()
        {
            HashSet<string> options = new();
            foreach (var option in _transaction.Options)
            {
                if (options.Add(option) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}