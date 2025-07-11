using BlockchainNode.Data;
using BlockchainNode.Tools;
using Microsoft.Extensions.Options;
using StructEventSystem;
using System.Security.Cryptography;
using System.Text;

namespace MainBlockchain
{
    public class PollManager : IDisposable, EventListener<BlockchainRefreshedEvent>
    {
        private readonly Dictionary<string, Poll> _polls;
        private readonly Blockchain _blockchain;
        private readonly Wallet _wallet;

        public IReadOnlyDictionary<string, Poll> Polls => _polls;

        public PollManager(Wallet wallet, Blockchain blockchain)
        {
            _wallet = wallet;
            _blockchain = blockchain;
            _polls = new();
            this.StartListening<BlockchainRefreshedEvent>();

            //TestPoseidon();
        }

        //private async void TestPoseidon()
        //{
        //    var response = await HttpService.GetPoseidonHashAsync(new string[] { "1231284719284", "151"});
        //    Console.WriteLine(response);
        //}

        public async Task<(bool, string)> TryCreatePoll(CreatePollTransaction transaction)
        {
            //if (_blockchain.TryAddPendingTransaction(transaction) == false)
            //{
            //    Console.WriteLine("Не далось создать голосование, невалидная транзакция");
            //    return false;
            //}
            //_blockchain.MinePendingTransactions();

            Poll poll = null;
            Random random = new Random();
            int randomId;
            while (true)
            {
                randomId = new Random().Next(int.MaxValue);
                if (_polls.ContainsKey(randomId.ToString()) == false)
                    break;
            }
            var options = transaction.CreatePollOptions();
            if (transaction.IsPrivate)
            {
                poll = await PrivatePoll.Builder.Build(randomId.ToString(), transaction.PollTitle, options, transaction.FromAddress, transaction.InvitedUsers, transaction.TokensAmount);
            }
            else
            {
                poll = new Poll(randomId.ToString(), transaction.PollTitle, options, transaction.FromAddress);
            }
            _polls.Add(randomId.ToString(), poll);
            return (true, randomId.ToString());
        }
        public async Task<(bool result, int weight)> TryRegister(ConfirmParticipation confirmParticipationData)
        {
            if (_polls.TryGetValue(confirmParticipationData.PollId, out var poll) == false
                || poll is PrivatePoll privatePoll == false)
                return (false, 0);
            var registrationResult = await privatePoll.TryRegisterUser(confirmParticipationData.FromAddress, confirmParticipationData.Sh);
            return registrationResult;
        }
        public async Task<bool> TryFinishPoll(FinishPollTransaction transaction)
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
            if (poll is PrivatePoll privatePoll)
            {
                Dictionary<int, List<(string c1x, string c1y, string c2x, string c2y)>> prepearedData = new();
                foreach (var item in privatePoll.Voted.Values)
                {
                    if (prepearedData.ContainsKey(item.OptionId) == false)                    
                        prepearedData.Add(item.OptionId, new List<(string, string, string, string)>());
                    prepearedData[item.OptionId].Add(new (item.C1x, item.C1y, item.C2x, item.C2y));
                }
                var results = await HttpService.CalculateResultAsync(prepearedData, privatePoll.SK);
                Console.WriteLine($"-----------Результаты {privatePoll.PollId}----------- ");
                foreach (var item in results)
                {
                    Console.WriteLine($"Опция {item.Key}: {item.Value}");
                }
                privatePoll.Finish(results);
                return true;
            }
            poll.Finish();
            //if (_blockchain.TryAddPendingTransaction(transaction) == false)
            //{
            //    return false;
            //}
            
            //_blockchain.MinePendingTransactions();
            return true;
        }

        public bool TryVoteAnonymously(VotePayload votePayload)
        {
            if (ValidationHandler.IsValid(votePayload) == false)
                return false;
            _polls.TryGetValue(votePayload.PollId, out var poll);
            var privatePoll = poll as PrivatePoll;
            privatePoll.Vote(votePayload); 
            return true;
        }
        public bool TryVoteUsual(VoteTransaction voteTransaction)
        {
            if (ValidationHandler.IsValid(voteTransaction) == false)
                return false;
            if (_polls.TryGetValue(voteTransaction.PollId, out var poll) == false)
                return false;
            return poll.TryVote(voteTransaction.OptionId, voteTransaction.FromAddress);
        }

        public void OnEvent(BlockchainRefreshedEvent eventType)
        {
            Console.WriteLine("Refreshing polls");
            RestorePolls();
        }
        private void RestorePolls()
        {
            //_polls.Clear();
            //List<GetVoteSignatureTransaction> votes = new();
            //List<FinishPollTransaction> finish = new();
            //foreach (var block in _blockchain.Chain)
            //{
            //    foreach (var transaction in block.Transactions)
            //    {
            //        if (transaction is CreatePollTransaction createPollTransaction)
            //        {
            //            var options = createPollTransaction.CreatePollOptions();
            //            var poll = new Poll(createPollTransaction.PollTitle, options, transaction.FromAddress);
            //            _polls.Add(transaction.TransactionId, poll);
            //        }
            //        if (transaction is GetVoteSignatureTransaction voteTransaction)
            //            votes.Add(voteTransaction);
            //        if (transaction is FinishPollTransaction finishPollTransaction)
            //            finish.Add(finishPollTransaction);
            //    }
            //}

            //foreach (var voteTransaction in votes)
            //{
            //    try
            //    {
            //        if (_polls.TryGetValue(voteTransaction.PollId, out var poll) == false)
            //            throw new System.Exception();
            //        poll.Vote(voteTransaction.ToAddress, voteTransaction.FromAddress);
            //    }
            //    catch
            //    {
            //        throw new System.Exception("Unable to restore polls");
            //    }
            //}

            //foreach (var finishTransaction in finish)
            //{
            //    try
            //    {
            //        if (_polls.TryGetValue(finishTransaction.PollId, out var poll) == false)
            //            throw new System.Exception();
            //        poll.Finish();
            //    }
            //    catch
            //    {
            //        throw new System.Exception("Unable to restore polls");
            //    }
            //}
        }
        public void Dispose()
        {
            this.StopListening<BlockchainRefreshedEvent>();
        }


    }
}