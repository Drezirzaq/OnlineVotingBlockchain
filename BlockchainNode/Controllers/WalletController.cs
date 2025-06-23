using Microsoft.AspNetCore.Mvc;

namespace MainBlockchain
{
    [ApiController]
    [Route("api/wallet")]
    public class WalletController : ControllerBase
    {
        public const int CREATE_AMOUNT = 120;
        private readonly Blockchain _blockchain;
        private readonly Wallet _wallet;
        public WalletController(Blockchain blockchain, Wallet wallet)
        {
            _blockchain = blockchain;
            _wallet = wallet;
        }

        [HttpGet("create")]
        public IActionResult CreateWallet([FromQuery] string address)
        {
            if (_wallet.TryCreateWallet(address, CREATE_AMOUNT) == false)
                return BadRequest("Unable to create wallet");
            return Ok();
        }

        [HttpGet("balance")]
        public IActionResult GetWalletBalance([FromQuery] string address)
        {
            var balance = _wallet.GetBalance(address);
            return Ok(new { Balance = balance });
        }

        [HttpPost("transfer")]
        public IActionResult CreateTransaction([FromBody] TransferTransaction transactionData)
        {
            if (_blockchain.TryAddTransactionAndMineBlock(transactionData) == false)
                return BadRequest("Invalid transaction");
            return Ok();
        }
        [HttpGet("addresses")]
        public IActionResult GetAddresses()
        {            
            return Ok(_wallet.Addresses);
        }
    }
}