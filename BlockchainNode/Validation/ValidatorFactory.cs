namespace MainBlockchain
{
    public class ValidatorFactory
    {
        private readonly Wallet _wallet;
        private readonly Blockchain _blockChain;
        public ValidatorFactory(Wallet wallet, Blockchain blockchain)
        {
            _wallet = wallet;
            _blockChain = blockchain;
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
            _ => throw new System.Exception()
        };
    }
}