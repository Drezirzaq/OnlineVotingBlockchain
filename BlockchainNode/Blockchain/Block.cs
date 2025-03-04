using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace MainBlockchain
{
    public class Block : IValidatable
    {
        [JsonIgnore]
        public string ValidationId => "Block";
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; }

        [JsonIgnore]
        public string CHash => CalculateHash();

        public Block(int index, DateTime timestamp, List<Transaction> transactions, string previousHash)
        {
            Index = index;
            Timestamp = timestamp;
            Transactions = transactions;
            PreviousHash = previousHash;
            Nonce = 0;

            Hash = CalculateHash();
        }

        public string CalculateHash()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string rawData = $"{Nonce}{Index}{TransactionsToString()}{PreviousHash}{Timestamp}";
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
        private string TransactionsToString()
        {
            var sorted = Transactions.OrderBy(x => x.Timestamp);
            var transactionStrings = new List<string>();
            foreach (var transaction in Transactions)
            {
                transactionStrings.Add(transaction.TransactionId);
            }
            return string.Join("", transactionStrings);
        }
        public void MineBlock(int difficulty)
        {
            string target = new string('0', difficulty);
            while (Hash.Substring(0, difficulty) != target)
            {
                Nonce++;
                Hash = CalculateHash();
            }
            Console.WriteLine($"Block mined: {Hash}");
        }
    }
}