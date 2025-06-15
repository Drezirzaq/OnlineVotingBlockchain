namespace MainBlockchain
{
    public class ValidatorFactory
    {
        private readonly Wallet _wallet;
        private readonly Blockchain _blockChain;
        private readonly PollManager _pollManager;
        public ValidatorFactory(PollManager pollManager, Wallet wallet, Blockchain blockchain)
        {
            _wallet = wallet;
            _blockChain = blockchain;
            _pollManager = pollManager;
        }
        public IEnumerable<IValidator> Create(IValidatable validatable) => validatable.ValidationId switch
        {
            "SystemTransaction" => new IValidator[0],
            "Block" => new IValidator[] {
                new BlockIndexValidator(validatable, _blockChain),
                new BlockHashValidator(validatable, _blockChain),
                new BlockPOWValidator(validatable, _blockChain),
                new BlockTransactionsValidator(validatable, _blockChain),
                new BlockTransactionsBalanceValidator(validatable, _wallet)
            },
            "TransferTransaction" => new IValidator[]
            {
                new TransferTransactionFormatValidator(validatable),
                new TransactionSignatureValidator(validatable),
                new TransactionBalanceValidator(validatable, _wallet),
                new WalletAddresValidator(validatable, _pollManager)
            },
            "CreatePollTransaction" => new IValidator[]
            {
                new TransactionSignatureValidator(validatable),
                new TransactionBalanceValidator(validatable, _wallet),
                new CreatePollTransactionValidator(validatable),
            },
            "FinishPollTransaction" => new IValidator[]
            {
                    new TransactionSignatureValidator(validatable),
                    new FinishPollValidator(validatable, _blockChain),
            },
            "VoteTransaction" => new IValidator[]
            {
                new TransferTransactionFormatValidator(validatable),
                new TransactionSignatureValidator(validatable),
                new VoteTransactionValidator(validatable, _blockChain),
                new TransactionBalanceValidator(validatable, _wallet),
            },
            "SignBlindedTransaction" => new IValidator[]
            {
                new TransactionSignatureValidator(validatable),
            },
            "ConfirmParticipationTransaction" => new IValidator[]
            {
                new TransactionSignatureValidator(validatable),
            },
            "FinishRegistrationTransaction" => new IValidator[]
            {
                new TransactionSignatureValidator(validatable),
            },
            "VotePayload" => new IValidator[]
            {
                new VotePayloadValidator(validatable, _pollManager)
            },
            _ => throw new System.Exception($"Validator doesn't impemented or registred for {validatable.ValidationId}")
        };
    }
}