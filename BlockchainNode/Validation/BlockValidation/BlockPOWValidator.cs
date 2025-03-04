namespace MainBlockchain
{
    public class BlockPOWValidator : IValidator
    {
        private readonly Block _block;
        private readonly Blockchain _blockchain;

        public BlockPOWValidator(IValidatable validatable, Blockchain blockchain)
        {
            _blockchain = blockchain;
            if (validatable is Block block)
            {
                _block = block;
                return;
            }
            throw new System.Exception("Wrong vaidator for block created");
        }
        public bool Validate()
        {
            string target = new string('0', _blockchain.Difficulty);
            return _block.Hash.StartsWith(target);
        }
    }
}