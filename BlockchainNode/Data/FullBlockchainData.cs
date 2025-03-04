namespace MainBlockchain
{
    public class FullBlockchainData
    {
        public List<Block> Chain { get; set; }
        public List<Transaction> PendingTransactions { get; set; }
    }
}