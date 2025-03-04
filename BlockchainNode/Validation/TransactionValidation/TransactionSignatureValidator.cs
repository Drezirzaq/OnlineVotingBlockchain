using System.Security.Cryptography;
using System.Text;

namespace MainBlockchain
{
    public class TransactionSignatureValidator : IValidator
    {
        private readonly Transaction _transaction;
        public TransactionSignatureValidator(IValidatable validatable)
        {
            if (validatable is Transaction transaction)
            {
                _transaction = transaction;
                return;
            }
            throw new System.Exception("Создан неверный валидатор для IValidatable");
        }
        public virtual bool Validate()
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(_transaction.GetRawData());
            byte[] signatureBytes = Convert.FromBase64String(_transaction.Signature);
            bool isValid = false;
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(_transaction.PublicKey), out _);
                isValid = rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            if (!isValid)
            {
                Console.WriteLine("Не удалось проверить подпись.");
                return false;
            }
            return true;
        }
    }
}