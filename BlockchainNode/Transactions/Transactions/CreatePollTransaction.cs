using System.Security.Cryptography;
using System.Text;

namespace MainBlockchain
{
    public class CreatePollTransaction : Transaction, IBalanceAffectingTransaction
    {
        public string PollTitle { get; set; }
        public string[] Options { get; set; }

        public string ToAddress => "system";
        public decimal Amount => 1;

        protected override string CombineData()
        {
            return $"{PollTitle}";//{string.Join(",", Options)}";
        }

        public IEnumerable<PollOption> CreatePollOptions()
        {
            List<PollOption> options = new();
            for (int i = 0; i < Options.Length; i++)
            {
                string id = string.Empty;
                using (SHA256 sha256 = SHA256.Create())
                {
                    string rawData = $"{TransactionId}{Options[i]}{i}";
                    byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                    id = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
                options.Add(new PollOption(Options[i], id));
            }
            return options;
        }
    }
}