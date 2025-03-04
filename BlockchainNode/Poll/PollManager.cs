using StructEventSystem;

namespace MainBlockchain
{
    public class PollManager : IDisposable, EventListener<BlockchainRefreshedEvent>
    {
        private readonly Dictionary<string, Poll> _polls;
        public IReadOnlyDictionary<string, Poll> Polls => _polls;
        private readonly Wallet _wallet;
        private readonly Blockchain _blockchain;
        public PollManager(Wallet wallet, Blockchain blockchain)
        {
            _wallet = wallet;
            _blockchain = blockchain;
            _polls = new();
            this.StartListening<BlockchainRefreshedEvent>();
        }

        public bool TryCreatePoll(CreatePollTransaction transaction)
        {
            if (_blockchain.TryAddPendingTransaction(transaction) == false)
            {
                Console.WriteLine("Не далось создать голосование");
                return false;
            }
            var options = transaction.CreatePollOptions();
            foreach (var option in options)
            {
                if (_wallet.TryCreateWallet(option.Id, 0) == false)
                    throw new System.Exception("Unxepected error");
            }
            _blockchain.MinePendingTransactions();
            var poll = new Poll(transaction.PollTitle, options, transaction.FromAddress);
            _polls.Add(transaction.TransactionId, poll);
            return true;
        }
        public bool TryFinishPoll(FinishPollTransaction transaction)
        {
            if (_polls.TryGetValue(transaction.PollId, out var poll) == false)
            {
                Console.WriteLine($"Голосование {transaction.PollId} не найдено.");
                return false;
            }
            if (poll.IsFinished)
            {
                Console.WriteLine("Голосование уже завершено.");
                return false;
            }
            if (transaction.FromAddress.Equals(poll.PollOwner) == false)
            {
                Console.WriteLine("Пользователь не может завершить голосование которое не начинал.");
                return false;
            }
            if (_blockchain.TryAddPendingTransaction(transaction) == false)
            {
                return false;
            }
            poll.Finish();
            _blockchain.MinePendingTransactions();
            return true;
        }
        public bool TryVote(VoteTransaction transaction)
        {
            if (_polls.TryGetValue(transaction.PollId, out var poll) == false)
            {
                Console.WriteLine($"Голосование {transaction.PollId} не найдено.");
                return false;
            }
            if (_blockchain.TryAddPendingTransaction(transaction) == false)
                return false;

            poll.Vote(transaction.ToAddress, transaction.FromAddress);

            _blockchain.MinePendingTransactions();
            return true;
        }
        public void OnEvent(BlockchainRefreshedEvent eventType)
        {
            Console.WriteLine("Refreshing polls");
            RestorePolls();
        }
        private void RestorePolls()
        {
            _polls.Clear();
            List<VoteTransaction> votes = new();
            List<FinishPollTransaction> finish = new();
            foreach (var block in _blockchain.Chain)
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction is CreatePollTransaction createPollTransaction)
                    {
                        var options = createPollTransaction.CreatePollOptions();
                        var poll = new Poll(createPollTransaction.PollTitle, options, transaction.FromAddress);
                        _polls.Add(transaction.TransactionId, poll);
                    }
                    if (transaction is VoteTransaction voteTransaction)
                        votes.Add(voteTransaction);
                    if (transaction is FinishPollTransaction finishPollTransaction)
                        finish.Add(finishPollTransaction);
                }
            }

            foreach (var voteTransaction in votes)
            {
                try
                {
                    if (_polls.TryGetValue(voteTransaction.PollId, out var poll) == false)
                        throw new System.Exception();
                    poll.Vote(voteTransaction.ToAddress, voteTransaction.FromAddress);
                }
                catch
                {
                    throw new System.Exception("Unable to restore polls");
                }
            }

            foreach (var finishTransaction in finish)
            {
                try
                {
                    if (_polls.TryGetValue(finishTransaction.PollId, out var poll) == false)
                        throw new System.Exception();
                    poll.Finish();
                }
                catch
                {
                    throw new System.Exception("Unable to restore polls");
                }
            }
        }

        public void Dispose()
        {
            this.StopListening<BlockchainRefreshedEvent>();
        }
    }
}