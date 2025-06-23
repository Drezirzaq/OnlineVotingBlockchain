using BlockchainNode.Data;
using BlockchainNode.Tools;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static MainBlockchain.PrivatePoll;

namespace MainBlockchain
{
    [ApiController]
    [Route("api/poll")]
    public class PollController : ControllerBase
    {
        private readonly PollManager _pollManager;
        public PollController(PollManager pollManager)
        {
            _pollManager = pollManager;
        }

        [HttpPost("create-poll")]
        public async Task<IActionResult> CreatePoll([FromBody] CreatePollTransaction transaction)
        {
            var pollCreated = await _pollManager.TryCreatePoll(transaction);
            if (pollCreated.Item1 == false)
                return BadRequest("Unable to create poll");
            return Ok(new { PollId = pollCreated.Item2 });
        }
        [HttpPost("confirm-registration")]
        public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmParticipation confirmParticipationData)
        {
            if (ValidationHandler.IsValid(confirmParticipationData) == false)
                return BadRequest("Unable to verify data");

            var registred = await _pollManager.TryRegister(confirmParticipationData);
            if (registred.result == false)
                return BadRequest("Unable to register user");
            return Ok(new
            {
                weight = registred.weight
            });
        }
        [HttpPost("finish-registration")]
        public async Task<IActionResult> FinishRegistration([FromBody] FinishRegistration finishRegistration)
        {
            if (ValidationHandler.IsValid(finishRegistration) == false)
                return BadRequest("Unable to verify data");

            if (_pollManager.Polls.TryGetValue(finishRegistration.PollId, out var poll) == false
               || poll is PrivatePoll privatePoll == false)
                return BadRequest("Wrong poll id");

            var result = await privatePoll.TryFinishRegistration(finishRegistration.FromAddress);

            if (result == false)
                return BadRequest("User unable to finish registraction");
            Console.WriteLine("Poll registration finished");
            return Ok();
        }

        [HttpPost("finish-poll")]
        public async Task<IActionResult> FinishPoll([FromBody] FinishPollTransaction transaction)
        {
            var result = await _pollManager.TryFinishPoll(transaction);
            if (result == false)
                return BadRequest("Unable to finish poll");
            return Ok();
        }
        [HttpGet("{id}/membership-tree-data")]
        public async Task<IActionResult> GetMembershipData(string id, [FromQuery] string commit)
        {
            if (_pollManager.Polls.TryGetValue(id, out var poll) == false
                || poll is not PrivatePoll privatePoll)
                return NotFound("poll");

            if (privatePoll.PollStatus != PrivatePollStatus.Voting)
                return BadRequest("voting not started");

            if (privatePoll.Commits.ContainsKey(commit) == false)
                return BadRequest("unknown commit");

            var merkleProof = privatePoll.GetMerkleProof(commit);

            return Ok(new
            {
                siblings = merkleProof.siblings,
                merklePath = merkleProof.merklePath,
                root = privatePoll.TreeRoot,
                pk_x = privatePoll.PK_X,
                pk_y = privatePoll.PK_Y
            });
        }

        [HttpGet("{id}/poll-results")]
        public async Task<IActionResult> GetPollResults(string id)
        {
            if (_pollManager.Polls.TryGetValue(id, out var poll) == false
                || poll is not PrivatePoll privatePoll)
                return NotFound("poll");

            if (privatePoll.PollStatus != PrivatePollStatus.Finished)
                return BadRequest("Voting not finished");

            return Ok(new
            {
                Results = privatePoll.Results
            });
        }


        [HttpPost("anonimus-vote")]
        public async Task<IActionResult> AnonimusVote([FromBody] VotePayload votePayload)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Recieved anonimus vote");
            Console.ForegroundColor = ConsoleColor.White;

            var result = await HttpService.VerifyProofs(votePayload);
            if (result)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Proofs verifyed!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to verify proofs!");
                Console.ForegroundColor = ConsoleColor.White;
            }

            if (_pollManager.TryVoteAnonymously(votePayload) == false)
                return BadRequest("Unable to vote");
           
            return Ok();
        }

        [HttpPost("usual-vote")]
        public IActionResult UsualVote([FromBody] VoteTransaction voteTransaction)
        {
            if (_pollManager.TryVoteUsual(voteTransaction) == false)
                return BadRequest();            
            return Ok();
        }

        [HttpGet("polls")]
        public IActionResult GetPolls()
        {
            List<PollData> pollsData = new();
            foreach (var poll in _pollManager.Polls)
            {
                pollsData.Add(new PollData()
                {
                    IsFinished = poll.Value.IsFinished,
                    PollId = poll.Key,
                    Title = poll.Value.PollTitle,
                    Options = poll.Value.Options.ToArray(),
                    IsPrivate = poll.Value.IsPrivate
                });
            }
            return Ok(pollsData);
        }
        [HttpPost("poll-details")]
        public IActionResult GetPollDetails([FromBody] GetPollRequest pollRequest)
        {
            Console.WriteLine("Get poll");

            if (_pollManager.Polls.TryGetValue(pollRequest.PollId, out var poll) == false)
            {
                return BadRequest("Poll not found");
            }
            PollData pollData = new PollData()
            {
                Votes = poll.Votes.ToDictionary(),
                IsFinished = poll.IsFinished,
                IsOwner = pollRequest.Address == poll.PollOwner,
                PollId = pollRequest.PollId,
                Title = poll.PollTitle,
                Options = poll.Options.ToArray(),
                IsPrivate = poll.IsPrivate,
                HasPermission = true,
            };
            if (poll.IsPrivate)
            {
                var privatePoll = (PrivatePoll)poll;
                var isInvited = privatePoll.IsInvited(pollRequest.Address);
                pollData.HasPermission = isInvited;
                pollData.PrivatePollStatus = (int)privatePoll.PollStatus;
            }
            return Ok(pollData);
        }
    }
}