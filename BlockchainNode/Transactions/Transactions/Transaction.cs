using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace MainBlockchain
{
    [JsonConverter(typeof(TransactionConverter))]
    public abstract class Transaction : IValidatable
    {
        [JsonIgnore]
        public virtual string ValidationId => TransactionType.ToString();
        public TransactionType TransactionType { get; set; }
        public string PublicKey { get; set; }
        public string FromAddress { get; set; }
        public string Signature { get; set; }
        public DateTime Timestamp { get; set; }

        [JsonIgnore]
        private string _transactionId = string.Empty;
        [JsonIgnore]
        public string TransactionId
        {
            get
            {
                if (string.IsNullOrEmpty(_transactionId))
                    _transactionId = CalculateTransactionId();
                return _transactionId;
            }
        }
        public string GetRawData()
        {
            var data = CombineData();
            Console.WriteLine(Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
            return $"{PublicKey}{FromAddress}{Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")}{CombineData()}";
        }
        protected abstract string CombineData();
        private string CalculateTransactionId()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string rawData = $"{TransactionType}{PublicKey}{FromAddress}{Signature}{Timestamp}{CombineData()}";
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}