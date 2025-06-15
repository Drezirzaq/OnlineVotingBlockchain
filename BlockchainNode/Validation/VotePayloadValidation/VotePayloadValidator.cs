using BlockchainNode.Data;

namespace MainBlockchain
{
    public class VotePayloadValidator : IValidator
    {
        private readonly VotePayload _votePayload;
        private readonly PollManager _pollManager;
        public VotePayloadValidator(IValidatable validatable, PollManager pollManager)
        {
            _pollManager = pollManager;
            if (validatable is VotePayload votePayload)
            {
                _votePayload = votePayload;
                return;
            }
            throw new System.Exception("Wrong vaidator for VotePayload created");
        }
        public bool Validate()
        {
            if (_pollManager.Polls.TryGetValue(_votePayload.PollId, out var poll) == false
                || poll is PrivatePoll privatePoll == false)
                return false;
            //подтверждает что в обоих доказательствах использовался одинаковый вес
            if (_votePayload.MembershipSignals.Contains(_votePayload.WeightHash) == false
                || _votePayload.VoteSignals.Contains(_votePayload.WeightHash) == false)
                return false;
            //подтверждает что optionId тот же что и в membershipProof
            if (_votePayload.MembershipSignals.Contains(_votePayload.OptionId.ToString()) == false)
                return false;
            //подтверждает что root использованный в proof действительно тот же что и реальный root голосования
            if (_votePayload.MembershipSignals.Contains(_votePayload.Root) == false 
                || privatePoll.TreeRoot != _votePayload.Root)
                return false;
            //проверяет что nullifier еще не зарегестрирован и что nullifier тот же что и в membershipProof
            if (_votePayload.MembershipSignals.Contains(_votePayload.Nullifier) == false
                || privatePoll.NullifierRegistrated(_votePayload.Nullifier))
                return false;
            //подтверждает что C1 и С2 те же что и в voteProof
            if (_votePayload.VoteSignals.Contains(_votePayload.C1x) == false
                || _votePayload.VoteSignals.Contains(_votePayload.C1y) == false
                || _votePayload.VoteSignals.Contains(_votePayload.C2x) == false
                || _votePayload.VoteSignals.Contains(_votePayload.C2y) == false)
                return false;
            return true;
        }
    }
}
