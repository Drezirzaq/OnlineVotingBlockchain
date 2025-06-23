namespace MainBlockchain
{
    public class GenesisTransaction : Transaction
    {       
        public GenesisTransaction()
        {
            TransactionType = TransactionType.GenesisTransaction;
            Timestamp = DateTime.Now;
        }
        protected override string CombineData()
        {
            return "";
        }
    }
}
