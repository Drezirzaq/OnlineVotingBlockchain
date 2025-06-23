namespace MainBlockchain
{
    public class Blockchain
    {
        public static event Action<Block> OnBlockMined;
        public static event Action<TransferTransaction> OnTransactionAdded;

        public List<Block> Chain { get; private set; }
        public int Difficulty { get; private set; }
        private const int MaxTransactionsPerBlock = 10;

        public Blockchain(int difficulty = 2)
        {
            Chain = new List<Block>();
            Difficulty = difficulty;
            Chain.Add(CreateGenesisBlock());
        }

        private Block CreateGenesisBlock()
        {
            return new Block(0, DateTime.Now, new GenesisTransaction(), "0");
        }

        public Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        public bool TryAddTransactionAndMineBlock(Transaction pendingTransaction)
        {
            if (ValidationHandler.IsValid(pendingTransaction) == false)
            {
                Console.WriteLine("Транзакция отклонена");
                return false;
            }

            Block newBlock = new Block(
                index: Chain.Count,
                timestamp: DateTime.Now,
                transaction: pendingTransaction,
                previousHash: GetLatestBlock().Hash
            );

            newBlock.MineBlock(Difficulty);

            if (TryAddBlock(newBlock) == false)
                return false;

            OnBlockMined?.Invoke(newBlock);
            return true;
        }
        private bool TryAddBlock(Block block)
        {
            if (ValidationHandler.IsValid(block) == false)
            {
                Console.WriteLine("Блок не прошел проверку и не был добавлен в цепочку.");
                return false;
            }
            Chain.Add(block);
            Console.WriteLine("Блок добавлен в блокчейн.");
            return true;
        }
        public void UpdateBlockchain(List<Block> newChain)
        {
            if (newChain == null || newChain.Count <= Chain.Count)
                return;
            var prevChain = Chain;
            Chain = new List<Block>();
            Chain.Add(newChain[0]);
            for (int i = 1; i < newChain.Count; i++)
            {
                if (ValidationHandler.IsValid(newChain[i]) == false)
                {
                    Chain = prevChain;
                    return;
                }
                Chain.Add(newChain[i]);
            }

        }
    }
}
