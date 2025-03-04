namespace MainBlockchain
{
    public static class SystemTransactionFactory
    {
        public static TransferTransaction Create(string toAddress, decimal amount)
        {
            return new TransferTransaction()
            {
                TransactionType = TransactionType.SystemTransaction,
                FromAddress = "system",
                ToAddress = toAddress,
                PublicKey = string.Empty,
                Signature = string.Empty,
                Amount = amount,
                Timestamp = DateTime.UtcNow,
            };
        }
    }
}