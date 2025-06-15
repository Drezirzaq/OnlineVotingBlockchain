namespace MainBlockchain
{
    public class WalletAddresValidator : IValidator
    {
        private readonly PollManager _pollManager;
        private readonly IBalanceAffectingTransaction _balanceTransaction;
        public WalletAddresValidator(IValidatable validatable, PollManager pollManager)
        {
            _pollManager = pollManager;
            if (validatable is IBalanceAffectingTransaction balanceAffectingTransaction)
            {
                _balanceTransaction = balanceAffectingTransaction;
                return;
            }
            throw new System.Exception("Создан неверный валидатор для IValidatable");
        }
        public bool Validate()
        {
            HashSet<string> optionsIds = new();
            foreach (var poll in _pollManager.Polls.Values)
            {
                foreach (var pollOption in poll.Options)
                {
                    optionsIds.Add(pollOption.Id);
                }
            }
            return optionsIds.Contains(_balanceTransaction.ToAddress) == false;
        }
    }
}
