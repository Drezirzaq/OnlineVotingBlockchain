using BlockchainNode.Data;
using BlockchainNode.Tools;
using NBitcoin.Secp256k1;
using PoseidonSharp;
using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;

namespace MainBlockchain
{
    public class PrivatePoll : Poll
    {
        public enum PrivatePollStatus
        {
            Registration = 0,
            Voting = 1,
            Finished = 2
        }

        private readonly HashSet<string> _invitedUsers;
        private readonly HashSet<string> _registredUsers;
        private readonly Dictionary<string, int> _commits;
        private readonly Dictionary<string, VotePayload> _voted;
        private readonly Dictionary<int, int> _results;

        public IEnumerable<string> InvitedUsers => _invitedUsers;
        public IReadOnlyDictionary<string, int> Commits => _commits;
        public IReadOnlyDictionary<string, VotePayload> Voted => _voted;
        public IReadOnlyDictionary<int, int> Results => _results;
        public override bool IsPrivate => true;

        public PrivatePollStatus PollStatus { get; private set; }
        public int TotalTokensAvailable { get; private set; }
        public string SK { get; private set; }
        public string PK_X { get; private set; }
        public string PK_Y { get; private set; }
        private List<List<string>> _tree;
        public string TreeRoot => _tree[_tree.Count - 1][0];
        public IReadOnlyList<IReadOnlyList<string>> Tree => _tree;

        private PrivatePoll(string pollId, string pollTitle, IEnumerable<PollOption> options,
            string owner, IEnumerable<string> invitedUsers, int tokensInPoll, string SK, string PK_X, string PK_Y) : base(pollId, pollTitle, options, owner)
        {
            _invitedUsers = new();
            foreach (var user in invitedUsers)
                _invitedUsers.Add(user);
            _invitedUsers.Add(owner);
            _registredUsers = new();
            _commits = new();
            _voted = new();
            _results = new();
            TotalTokensAvailable = tokensInPoll;
            PollStatus = PrivatePollStatus.Registration;

            this.SK = SK;
            this.PK_X = PK_X;
            this.PK_Y = PK_Y;

            Console.WriteLine($"Private poll successfully builded!");
            Console.WriteLine($"SK: {SK}");
            Console.WriteLine($"PK: {PK_X}");
            Console.WriteLine($"PK: {PK_Y}");
        }

        public async Task<(bool result, int weight)> TryRegisterUser(string userAddress, string sh)
        {
            if (_registredUsers.Contains(userAddress)
                || _invitedUsers.Contains(userAddress) == false)
                return (false, 0);
            var weight = CalculateVoteWeight(userAddress);
            var commit = await HttpService.GetPoseidonHashAsync(new string[] {sh, weight.ToString() });
            Console.WriteLine($"Commit: {commit}");
            if (_commits.TryAdd(commit, weight) == false)
                return (false, 0);
            _registredUsers.Add(userAddress);
            Console.WriteLine($"User {userAddress} registred\n Commit: {commit}");
            return (true, weight);
        }
        public void Vote(VotePayload votePayload)
        {
            if (NullifierRegistrated(votePayload.Nullifier)) 
                return;
            _voted.Add(votePayload.Nullifier, votePayload);
        }
        public async Task<bool> TryFinishRegistration(string fromAddress)
        {
            if (PollOwner != fromAddress || PollStatus != PrivatePollStatus.Registration)
                return false;
            PollStatus = PrivatePollStatus.Voting;
            _tree = await HttpService.BuildMerkleTree(_commits.Keys.ToArray());
            return true;
        }
        private int CalculateVoteWeight(string address)
        {
            Random random = new Random();
            return random.Next(200);
        }

        public void CloseRegistrationPhase()
        {

        }
        public void Finish(Dictionary<int, int> results)
        {
            if (PollStatus != PrivatePollStatus.Voting)
                return;
            Finish();
            PollStatus = PrivatePollStatus.Finished;
            foreach (var item in results)
            {
                _results.TryAdd(item.Key, item.Value);
            }
        }

        public bool IsInvited(string address) => _invitedUsers.Contains(address);
        public bool NullifierRegistrated(string nullifier) => _voted.ContainsKey(nullifier);

        public (List<string> siblings, List<int> merklePath) GetMerkleProof(string commit)
        {
            if (_tree == null || _tree.Count == 0)
                throw new ArgumentException("Levels list is empty");

            int idx = _tree[0].FindIndex(leaf => leaf == commit);
            if (idx == -1)
                throw new ArgumentException("Commit not found in leaf level");

            var siblings = new List<string>();
            var pathBits = new List<int>();

            for (int lvl = 0; lvl < _tree.Count - 1; lvl++)
            {
                bool isRight = (idx & 1) == 1;
                int pairIdx = isRight ? idx - 1 : idx + 1;
                string sibling = pairIdx < _tree[lvl].Count ? _tree[lvl][pairIdx] : "0";
                siblings.Add(sibling);
                pathBits.Add(isRight ? 1 : 0);
                idx >>= 1;
            }

            return (siblings, pathBits);
        }

        public static class Builder
        {
            public static async Task<PrivatePoll> Build(string pollId, string pollTitle, IEnumerable<PollOption> options,
                string owner, IEnumerable<string> invitedUsers, int tokensInPoll)
            {
                var keys = await HttpService.GetElGamalKeysAsync();
                var poll = new PrivatePoll(pollId, pollTitle, options, owner, invitedUsers, tokensInPoll, keys.SK, keys.PK.X, keys.PK.Y);
                return poll;
            }
        }
    }

    
}
