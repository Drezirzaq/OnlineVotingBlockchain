using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MainBlockchain
{
    public class Poll
    {
        public string PollId { get; private set; }
        public bool IsFinished { get; private set; }
        public string PollTitle { get; private set; }
        public string PollOwner { get; private set; }

        public virtual bool IsPrivate => false;
        [JsonIgnore]
        private readonly IEnumerable<PollOption> _options;
        public IEnumerable<PollOption> Options => _options;

        private Dictionary<string, int> _votes;
        public IReadOnlyDictionary<string, int> Votes => _votes;
        private HashSet<string> _voted;

        public Poll(string pollId, string pollTitle, IEnumerable<PollOption> options, string owner)
        {
            PollId = pollId;
            PollTitle = pollTitle;
            PollOwner = owner;
            _options = options;
            _voted = new();
            _votes = new();
            foreach (var item in options)
                _votes.Add(item.Id, 0);
        }
        public bool TryVote(string optionId, string address)
        {
            if (_votes.ContainsKey(optionId) == false || _voted.Add(address) == false)
            {
                //throw new System.Exception($"Unable to vote, no option with id {optionId}");
                return false;
            }
            _votes[optionId]++;
            return true;
        }

        public virtual void Finish()
        {
            IsFinished = true;
        }
    }
}