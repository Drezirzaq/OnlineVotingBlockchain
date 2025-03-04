using Microsoft.AspNetCore.Mvc;

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
        public IActionResult CreatePoll([FromBody] CreatePollTransaction transaction)
        {
            if (_pollManager.TryCreatePoll(transaction) == false)
                return BadRequest("Unable to create poll");
            return Ok();
        }
        [HttpPost("finish-poll")]
        public IActionResult FinishPoll([FromBody] FinishPollTransaction transaction)
        {
            if (_pollManager.TryFinishPoll(transaction) == false)
                return BadRequest("Unable to finish poll");
            return Ok();
        }
        [HttpPost("vote")]
        public IActionResult Vote([FromBody] VoteTransaction transaction)
        {
            if (_pollManager.TryVote(transaction) == false)
                return BadRequest("Unable to finish poll");
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
                    Options = poll.Value.Options.ToArray()
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
            return Ok(new PollData()
            {
                Votes = poll.Votes,
                IsFinished = poll.IsFinished,
                IsOwner = pollRequest.Address == poll.PollOwner,
                PollId = pollRequest.PollId,
                Title = poll.PollTitle,
                Options = poll.Options.ToArray()
            });
        }
    }
}