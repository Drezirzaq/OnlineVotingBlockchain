using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MainBlockchain
{
    public class TransactionConverter : JsonConverter<Transaction>
    {
#pragma warning disable CS8632
        public override Transaction? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
#pragma warning restore CS8632
        {
            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
            {
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("transactionType", out JsonElement transactionTypeElement))
                {
                    throw new JsonException("Поле 'TransactionType' отсутствует в JSON.");
                }

                TransactionType transactionType = (TransactionType)transactionTypeElement.GetInt32();

                Transaction transaction = transactionType switch
                {
                    TransactionType.SystemTransaction => new TransferTransaction(),
                    TransactionType.TransferTransaction => new TransferTransaction(),
                    TransactionType.FinishPollTransaction => new FinishPollTransaction(),
                    TransactionType.CreatePollTransaction => new CreatePollTransaction(),
                    TransactionType.VoteTransaction => new VoteTransaction(),
                    _ => throw new JsonException($"Неизвестный тип транзакции: {transactionType}")
                };
                transaction = (Transaction)JsonSerializer.Deserialize(root.GetRawText(), transaction.GetType(), options)!;
                return transaction;
            }
        }

        public override void Write(Utf8JsonWriter writer, Transaction value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, value.GetType(), options);
        }
    }



}