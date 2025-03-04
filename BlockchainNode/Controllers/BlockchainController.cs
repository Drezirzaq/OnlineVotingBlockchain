using Microsoft.AspNetCore.Mvc;
using StructEventSystem;

namespace MainBlockchain
{
    [ApiController]
    [Route("api/blockchain")]
    public partial class BlockchainController : ControllerBase
    {
        private readonly Node _node;
        public BlockchainController(Node node)
        {
            _node = node;
        }

        [HttpGet("full")]
        public IActionResult GetFullBlockchain()
        {
            var blockchainData = new FullBlockchainData()
            {
                Chain = _node.Blockchain.Chain,
                PendingTransactions = _node.Blockchain.PendingTransactions
            };

            return Ok(blockchainData);
        }
        [HttpGet("status")]
        public IActionResult GetNodeStatus()
        {
            var status = new
            {
                NodeAddress = _node.Address,
                TotalBlocks = _node.Blockchain.Chain.Count,
                PendingTransactionsCount = _node.Blockchain.PendingTransactions.Count,
                ConnectedPeers = _node.PeerManager.GetPeers().Count
            };

            return Ok(status);
        }
        [HttpGet("blocks")]
        public IActionResult GetAllBlocks()
        {
            return Ok(_node.Blockchain.Chain);
        }

        [HttpPost("add-block")]
        [Consumes("application/json")]
        public IActionResult ReceiveBlock([FromBody] Block block)
        {
            if (_node.Blockchain.TryAddBlock(block) == false)
                return Conflict();
            EventManager.TriggerEvent<BlockchainRefreshedEvent>(new BlockchainRefreshedEvent());
            return Ok();
        }
        [HttpPost("transaction/receive")]
        public IActionResult ReceiveTransaction([FromBody] TransferTransaction transaction)
        {
            if (!BlockchainUtilities.ValidateTransaction(transaction, _node.Blockchain))
            {

                return BadRequest();
            }
            return Ok(new { Message = "" });
        }

        [HttpGet("transactions/pending")]
        public IActionResult GetPendingTransactions()
        {
            return Ok(_node.Blockchain.PendingTransactions);
        }
    }
}