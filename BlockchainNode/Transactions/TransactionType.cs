namespace MainBlockchain
{
    public enum TransactionType
    {
        TransferTransaction = 0,
        CreatePollTransaction = 1,
        VoteTransaction = 2,
        SystemTransaction = 4,
        FinishPollTransaction = 5
    }
}