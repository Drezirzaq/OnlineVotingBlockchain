namespace MainBlockchain
{
    public enum TransactionType
    {
        TransferTransaction = 0,
        CreatePollTransaction = 1,
        VoteTransaction = 2,
        SystemTransaction = 4,
        FinishPollTransaction = 5,
        SignBlindedTransaction = 6,
        AnonimusVoteTransaction = 7,
        ConfirmParticipationTransaction = 8,
        FinishRegistrationTransaction = 9
    }
}