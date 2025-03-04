using Newtonsoft.Json;
using System.Text;

namespace MainBlockchain
{
    public class PeerManager
    {
        private readonly List<string> _peers;
        private readonly string _nodeAddress;
        private readonly HttpClient _httpClient;
        private readonly string _registryServerUrl = "http://192.168.1.54:5010/registry"; //"http://localhost:5010/registry";
        private readonly ILogger _logger;
        private readonly object _lock = new object();
        private CancellationTokenSource _cancellationTokenSource;
        public PeerManager(ILogger logger, string nodeAddress)
        {
            _logger = logger;
            _peers = new List<string>();
            _nodeAddress = nodeAddress;
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
            StartPeerMonitoring();
        }


        public void StartPeerMonitoring()
        {
            Task.Run(async () => await MonitorPeersAsync(_cancellationTokenSource.Token));
        }


        private async Task MonitorPeersAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await UpdatePeerListAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ошибка при мониторинге пиров: {Message}", ex.Message);
                }

                await Task.Delay(10000, cancellationToken);
            }

            _logger.LogInformation("Мониторинг пиров остановлен.");
        }

        public void StopPeerMonitoring()
        {
            _cancellationTokenSource.Cancel();
        }

        public async Task RegisterAsync()
        {
            var request = new { Address = _nodeAddress };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_registryServerUrl}/register", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Узел успешно зарегистрирован.");
                    await UpdatePeerListAsync();
                }
                else
                {
                    _logger.LogInformation("Ошибка регистрации узла: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception)
            {
                _logger.LogInformation("Ошибка при попытке регистрации узла.");
            }
        }

        public async Task UnregisterAsync()
        {
            var request = new { Address = _nodeAddress };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{_registryServerUrl}/unregister", content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Узел успешно удалён из реестра.");
                }
                else
                {
                    _logger.LogInformation("Ошибка удаления узла из реестра: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception)
            {
                _logger.LogInformation("Ошибка при попытке удаления узла из реестра.");
            }
        }

        public async Task UpdatePeerListAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_registryServerUrl}/list");
                if (response.IsSuccessStatusCode)
                {
                    var peerListJson = await response.Content.ReadAsStringAsync();
                    var peers = JsonConvert.DeserializeObject<List<string>>(peerListJson);
                    lock (_lock)
                    {
                        _peers.Clear();
                        if (peers != null)
                            _peers.AddRange(peers.Where(p => p != _nodeAddress));
                    }
                    //_logger.LogInformation($"Список пиров обновлён, количество подключений: {_peers.Count}");
                }
                else
                {
                    _logger.LogInformation("Ошибка при получении списка пиров: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception _)
            {
                _logger.LogInformation("Ошибка при попытке получить список пиров.");
            }
        }

        public void HandleNodeNotification(string address, string action)
        {
            if (action == "connected" && !_peers.Contains(address))
            {
                _peers.Add(address);
                _logger.LogInformation("Добавлен новый пир: {Address}", address);
            }
            else if (action == "disconnected" && _peers.Contains(address))
            {
                _peers.Remove(address);
                _logger.LogInformation("Пир отключён: {Address}", address);
            }
        }

        public List<string> GetPeers()
        {
            lock (_lock)
            {
                return new List<string>(_peers);
            }
        }
    }

}