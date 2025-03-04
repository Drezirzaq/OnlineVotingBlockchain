namespace MainBlockchain
{
    public class BlockIndexValidator : IValidator
    {
        private readonly Block _block;
        private readonly Blockchain _blockchain;

        public BlockIndexValidator(IValidatable validatable, Blockchain blockchain)
        {
            _blockchain = blockchain;
            if (validatable is Block block)
            {
                _block = block;
                return;
            }
            throw new System.Exception("Wrong vaidator for block created");
        }

        public bool Validate() => _block.Index == _blockchain.GetLatestBlock().Index + 1;
    }
}