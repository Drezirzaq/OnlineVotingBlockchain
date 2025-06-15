using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MainBlockchain
{
    public class Poll
    {
        public string PollId { get; private set; }
        public bool IsFinished { get; private set; }
        public string PollTitle { get; private set; }
        public string PollOwner { get; private set; }
        public string PublicKeyPem { get; private set; }
        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }
        public virtual bool IsPrivate => false;
        [JsonIgnore]
        private readonly IEnumerable<PollOption> _options;
        public IEnumerable<PollOption> Options => _options;

        private HashSet<string> _givenTokens;

        private Dictionary<string, int> _votes;
        public Dictionary<string, int> Votes => _votes;
        public Poll(string pollId, string pollTitle, IEnumerable<PollOption> options, string owner)
        {
            PollId = pollId;
            _options = options;
            PollTitle = pollTitle;
            PollOwner = owner;
            _givenTokens = new();
            _votes = new();
            foreach (var item in options)
                _votes.Add(item.Id, 0);

            var keys = RsaKeyPair.GenerateRsaKeys();
            PublicKeyPem = keys.PublicKeyPem;
            PrivateKey = keys.PrivateKeyBase64;
            PublicKey = keys.PublicKeyBase64;
        }
        public void Vote(string optionId, int tokens)
        {
            if (_votes.ContainsKey(optionId) == false)
                throw new System.Exception($"Unable to vote, no option with id {optionId}");
            _votes[optionId]+= tokens;
        }
        public virtual string SignBlindedMessage(string fromAddress, string blindedMessageHex)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Starting signind...");
            Console.ForegroundColor = ConsoleColor.White;

            // 1. Преобразуем blindedMessage из hex в BigInteger
            var blindedBytes = Convert.FromHexString(blindedMessageHex);
            var blindedInt = new BigInteger(blindedBytes, isBigEndian: true, isUnsigned: true);

            // 2. Загружаем приватный ключ
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(PrivateKey), out _);

            // 3. Получаем параметры ключа
            var parameters = rsa.ExportParameters(true);
            var d = new BigInteger(parameters.D, isBigEndian: true, isUnsigned: true);
            var n = new BigInteger(parameters.Modulus, isBigEndian: true, isUnsigned: true);

            // 4. Вычисляем подпись: s' = (m')^d mod n
            var signedBlinded = BigInteger.ModPow(blindedInt, d, n);

            // 5. Преобразуем результат в байты (без знака, в big-endian)
            var signedBytes = signedBlinded.ToByteArray(isBigEndian: true, isUnsigned: true);

            var signed = Convert.ToHexString(signedBytes).ToLowerInvariant();

            if (_givenTokens.Add(fromAddress) == false)
                throw new System.Exception($"Signature already was given to address {fromAddress}");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Sign completed");
            Console.ForegroundColor = ConsoleColor.White;

            return signed;
        }
      

        public bool VerifySignature(string optionHashHex, string signedBlindedMessageHex)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Starting verifying signature...");
            Console.ForegroundColor = ConsoleColor.White;
            // Преобразуем хэш варианта ответа (optionHash) из hex в BigInteger
            var optionHashBytes = Convert.FromHexString(optionHashHex);
            var optionHashInt = new BigInteger(optionHashBytes, isBigEndian: true, isUnsigned: true);

            // Преобразуем разослепленное подписанное сообщение из hex в BigInteger
            var signedBytes = Convert.FromHexString(signedBlindedMessageHex);
            var signedInt = new BigInteger(signedBytes, isBigEndian: true, isUnsigned: true);

            // Загружаем публичный ключ
            using var rsa = RSA.Create();
            byte[] keyBytes = Convert.FromBase64String(PublicKey);
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

            // Получаем параметры ключа
            var parameters = rsa.ExportParameters(false);
            var n = new BigInteger(parameters.Modulus, isBigEndian: true, isUnsigned: true);
            var e = new BigInteger(parameters.Exponent, isBigEndian: true, isUnsigned: true);

            // Вычисляем: m = (s^e mod n)
            var calculatedMessage = BigInteger.ModPow(signedInt, e, n);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Message calculated");
            Console.ForegroundColor = ConsoleColor.White;
            if (optionHashInt.Equals(calculatedMessage))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Message verifyed");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Message is not verifyed");
                Console.ForegroundColor = ConsoleColor.White;
            }
            // Проверяем, что разослепленное сообщение совпадает с рассчитанным хэшем
            return optionHashInt.Equals(calculatedMessage);
        }
        public string SignPayload(object payload)
        {
            // Сериализуем payload в JSON (важно использовать стабильный формат)
            string json = JsonSerializer.Serialize(payload);
            byte[] data = Encoding.UTF8.GetBytes(json);

            // Хешируем payload
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(data);

            byte[] signature;
            // Загружаем RSA-ключ
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(PrivateKey), out _);

                // Подписываем хэш
                signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            // Возвращаем подпись в base64
            return Convert.ToBase64String(signature);
        }
        public bool VerifyPayloadSignature(object payload, string base64Signature)
        {
            // Сериализуем payload в тот же JSON-формат
            string json = JsonSerializer.Serialize(payload);
            byte[] data = Encoding.UTF8.GetBytes(json);

            // Хешируем данные
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(data);

            // Импортируем публичный ключ
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(PublicKey), out _);

            // Декодируем подпись
            byte[] signature = Convert.FromBase64String(base64Signature);

            // Проверяем подпись
            return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        public bool TryGetOptionId(string option, out string optionId)
        {
            optionId = _options.FirstOrDefault(x => x.Id.Equals(option)).Id;
            return optionId != null && optionId != string.Empty;
        }
        public virtual int GetVoteWeight(string address) => 1;

        public virtual void Finish()
        {
            IsFinished = true;
        }
    }
    public class RsaKeyPair
    {
        public string PublicKeyPem { get; set; }
        public string PublicKeyBase64 { get; set; }
        public string PrivateKeyBase64 { get; set; }
        public static RsaKeyPair GenerateRsaKeys()
        {
            using var rsa = RSA.Create(2048);

            var publicKey = rsa.ExportSubjectPublicKeyInfo();
            var privateKey = rsa.ExportRSAPrivateKey();

            string publicKeyPem = new StringBuilder()
                .AppendLine("-----BEGIN PUBLIC KEY-----")
                .AppendLine(Convert.ToBase64String(publicKey, Base64FormattingOptions.InsertLineBreaks))
                .AppendLine("-----END PUBLIC KEY-----")
                .ToString();

            return new RsaKeyPair
            {
                PublicKeyBase64 = Convert.ToBase64String(publicKey),
                PublicKeyPem = publicKeyPem,
                PrivateKeyBase64 = Convert.ToBase64String(privateKey)
            };
        }
    }
    
}