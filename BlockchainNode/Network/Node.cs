using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using StructEventSystem;

namespace MainBlockchain
{
    public class Node : IAsyncDisposable
    {
        public string Address { get; private set; }
        public PeerManager PeerManager { get; private set; }
        public Blockchain Blockchain { get; private set; }
        private ILogger _logger;
        public Node(Blockchain blockchain, string address, ILogger<PeerManager> logger)
        {
            _logger = logger;
            Address = address;
            Blockchain = blockchain;
            PeerManager = new PeerManager(logger, address);
            Blockchain.OnBlockMined += BroadcastBlock;
            Blockchain.OnTransactionAdded += BroadcastTransaction;
            Initialize();
        }

        public async void Initialize()
        {
            await PeerManager.RegisterAsync();
            await SyncBlockchainAsync();
        }

        public async Task SyncBlockchainAsync()
        {
            var peers = PeerManager.GetPeers();
            foreach (var peer in peers)
            {
                try
                {
                    var response = await new HttpClient().GetAsync($"{peer}/api/blockchain/full");
                    if (response.IsSuccessStatusCode)
                    {
                        var fullBlockchainJson = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions
                        {
                            Converters = { new TransactionConverter() },
                            PropertyNameCaseInsensitive = true,
                            WriteIndented = true
                        };
                        var fullBlockchainData = System.Text.Json.JsonSerializer.Deserialize<FullBlockchainData>(fullBlockchainJson, options);

                        var remoteChain = fullBlockchainData?.Chain;
                        // var pendingTransactions = fullBlockchainData?.PendingTransactions;

                        Blockchain.UpdateBlockchain(remoteChain);
                        // SyncPendingTransactions(pendingTransactions);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при синхронизации с {peer}: {ex.Message}");
                }
            }
            EventManager.TriggerEvent<BlockchainRefreshedEvent>(new BlockchainRefreshedEvent());
            Console.WriteLine($"Блокчейн успешно обновлен.");
            Console.WriteLine("Неподтвержденные транзакции синхронизированы.");
        }

        public async void BroadcastBlock(Block block) => await BroadcastToPeers("api/blockchain/add-block", block);
        private async void BroadcastTransaction(TransferTransaction transaction) => await BroadcastToPeers("api/transaction/receive", transaction);
        private async Task BroadcastToPeers(string endpoint, object data)
        {
            var peers = PeerManager.GetPeers();
            string jsonMessage = System.Text.Json.JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            //Console.WriteLine("Отправляемый JSON:");
            //Console.WriteLine(jsonMessage);
            var content = new StringContent(jsonMessage, Encoding.UTF8, "application/json");

            var tasks = peers.Select(async peer =>
            {
                try
                {
                    var response = await new HttpClient().PostAsync($"{peer}/{endpoint}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Данные успешно отправлены на {peer}/{endpoint}");
                    }
                    else
                    {
                        _logger.LogError($"Ошибка отправки данных на {peer}/{endpoint}: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка подключения к {peer}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }
        public async ValueTask DisposeAsync()
        {
            Blockchain.OnBlockMined -= BroadcastBlock;
            Blockchain.OnTransactionAdded -= BroadcastTransaction;
            PeerManager.StopPeerMonitoring();
            await PeerManager.UnregisterAsync();
            _logger.LogInformation("disposed");
        }
    }
}