namespace MainBlockchain
{
    public class BlockHashValidator : IValidator
    {
        private readonly Block _block;
        private readonly Blockchain _blockchain;

        public BlockHashValidator(IValidatable validatable, Blockchain blockchain)
        {
            _blockchain = blockchain;
            if (validatable is Block block)
            {
                _block = block;
                return;
            }
            throw new System.Exception("Wrong vaidator for block created");
        }

        public bool Validate() => _block.CalculateHash() == _block.Hash
            && _blockchain.GetLatestBlock().Hash == _block.PreviousHash;
    }
}