using System.Security.Cryptography;
using System.Text;
using NBitcoin;
using NBitcoin.Crypto;

namespace MainBlockchain
{
    public static class BlockchainUtilities
    {
        public static bool ValidateTransactionFormat(TransferTransaction transaction)
        {
            if (string.IsNullOrEmpty(transaction.FromAddress) || string.IsNullOrEmpty(transaction.ToAddress))
            {
                Console.WriteLine("Transaction addresses are invalid.");
                return false;
            }
            if (transaction.Amount <= 0)
            {
                Console.WriteLine("Transaction amount must be positive.");
                return false;
            }
            if (string.IsNullOrEmpty(transaction.PublicKey))
            {
                Console.WriteLine("Transaction public key is missing.");
                return false;
            }
            if (string.IsNullOrEmpty(transaction.Signature))
            {
                Console.WriteLine("Transaction signature is missing.");
                return false;
            }
            return true;
        }
        public static bool VerifySignature(string data, string signature, string publicKey)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = Convert.FromBase64String(signature);

            using (RSA rsa = RSA.Create())
            {
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKey), out _);
                bool isValid = rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                return isValid;
            }
        }
    }
}
