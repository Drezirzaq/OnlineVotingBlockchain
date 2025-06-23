using Microsoft.AspNetCore.Mvc;
using StructEventSystem;
using System.Text;

namespace MainBlockchain
{
    [ApiController]
    [Route("api/blockchain")]
    public partial class BlockchainController : ControllerBase
    {
        private readonly Node _node;
        private readonly StringBuilder _stringBuilder;
        public BlockchainController(Node node)
        {
            _node = node;
            _stringBuilder = new();
        }

        [HttpGet("last-block")]
        public IActionResult GetLastBlock()
        {
            //var blockchainData = new FullBlockchainData()
            //{
            //    Chain = _node.Blockchain.Chain,
            //    PendingTransactions = _node.Blockchain.PendingTransactions
            //};
            var block = _node.Blockchain.GetLatestBlock();
            _stringBuilder.Clear();
            _stringBuilder.AppendLine($"Timestamp: {block.Timestamp}");
            _stringBuilder.AppendLine($"Index: {block.Index}");
            _stringBuilder.AppendLine($"Hash: {block.Hash}");
            _stringBuilder.AppendLine("---Transaction---");
            _stringBuilder.AppendLine(block.Transaction.ToString());
            var result = _stringBuilder.ToString();
            _stringBuilder.Clear();
            return Ok(result);
        }

        [HttpGet("full")]
        public IActionResult GetFullBlockchain()
        {
            var blockchainData = new FullBlockchainData()
            {
                Chain = _node.Blockchain.Chain                
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
            //if (_node.Blockchain.TryAddBlock(block) == false)
            //    return Conflict();
            //EventManager.TriggerEvent<BlockchainRefreshedEvent>(new BlockchainRefreshedEvent());
            return Ok();
        }
        //[HttpPost("transaction/receive")]
        //public IActionResult ReceiveTransaction([FromBody] TransferTransaction transaction)
        //{
        //    //if (!BlockchainUtilities.ValidateTransaction(transaction, _node.Blockchain))
        //    //{
        //    //    return BadRequest();
        //    //}
        //    return Ok(new { Message = "" });
        //}
    }
}