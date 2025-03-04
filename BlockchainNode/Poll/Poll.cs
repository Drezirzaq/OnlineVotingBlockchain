using System.Text.Json.Serialization;

namespace MainBlockchain
{
    public class Poll
    {
        public bool IsFinished { get; private set; }
        public string PollTitle { get; private set; }
        public string PollOwner { get; private set; }
        [JsonIgnore]
        private readonly IEnumerable<PollOption> _options;
        public IEnumerable<PollOption> Options => _options;

        private Dictionary<string, HashSet<string>> _votes;
        public Dictionary<string, uint> Votes => _votes.ToDictionary(x => x.Key, x => (uint)x.Value.Count());
        public Poll(string pollTitle, IEnumerable<PollOption> options, string owner)
        {
            _options = options;
            PollTitle = pollTitle;
            PollOwner = owner;
            _votes = new();
            foreach (var item in options)
                _votes.Add(item.Id, new HashSet<string>());
        }
        public void Vote(string optionId, string voterAddress)
        {
            if (_votes.TryGetValue(optionId, out var votes) == false)
                throw new System.Exception($"Unable to vote, no option with id {optionId}");
            if (votes.Add(voterAddress) == false)
                throw new System.Exception("Double vote try detected");
        }

        public void Finish()
        {
            IsFinished = true;
        }
    }
}