using Microsoft.AspNetCore.Mvc;

namespace MainBlockchain
{
    [ApiController]
    [Route("api/node")]
    public class NodeController : ControllerBase
    {
        private readonly Node _node;
        public NodeController(Node node)
        {
            _node = node;
        }

        [HttpPost("notification")]
        public IActionResult RegistryNotification([FromBody] NodeNotification notification)
        {
            _node.PeerManager.HandleNodeNotification(notification.Address, notification.Action);
            return Ok(new { message = "Notification processed successfully." });
        }
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}