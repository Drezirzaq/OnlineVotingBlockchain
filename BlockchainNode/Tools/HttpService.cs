using BlockchainNode.Data;
using System.Text;
using System.Text.Json;

namespace BlockchainNode.Tools
{
    public static class HttpService
    {
        private static readonly HttpClient _httpClient;
        static HttpService()
        {
            _httpClient = new HttpClient();
        }

        public static async Task<(string SK, PKey PK)> GetElGamalKeysAsync()
        {
            using var client = new HttpClient();
            var request = new PoseidonRequest
            {
                Inputs = []
            };
            var response = await client.PostAsJsonAsync("http://192.168.1.54:8187/main/generate-keys", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<KeysResponse>();
            return (result.SK, result.PK);
        }
      
        public static async Task<string> GetPoseidonHashAsync(string[] inputs)
        {
            using var client = new HttpClient();

            var request = new PoseidonRequest
            {
                Inputs = inputs
            };

            var response = await client.PostAsJsonAsync("http://192.168.1.54:8187/main/poseidon-hash", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PoseidonResponse>();
            return result.Hash;
        }
        public static async Task<string> EncodeWeight(int weight)
        {
            using var client = new HttpClient();

            var request = JsonSerializer.Serialize(new
            {
                weight = weight
            });


            var response = await client.PostAsJsonAsync("http://192.168.1.54:8187/main/encode-weight", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EncodedWeight>();
            return result.Weight;
        }
        public static async Task<List<List<string>>> BuildMerkleTree(string[] commits)
        {
            using var client = new HttpClient();

            var response = await client.PostAsJsonAsync("http://192.168.1.54:8187/main/build-merkle-tree", commits);
            response.EnsureSuccessStatusCode();

            var tree = await response.Content.ReadFromJsonAsync<List<List<string>>>();
            return tree;
        }
        public static async Task<bool> VerifyProofs(VotePayload votePayload)
        {
            using var client = new HttpClient();

            var response = await client.PostAsJsonAsync("http://192.168.1.54:8187/main/verify-proofs", votePayload);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<bool>();
        }
        public static async Task<Dictionary<int, int>> CalculateResultAsync(Dictionary<int, List<(string, string, string, string)>> data, string sk)
        {
            using var client = new HttpClient();

            var request = new { data, sk };

            var options = new JsonSerializerOptions
            {
                IncludeFields = true 
            };

            var resp = await client.PostAsJsonAsync("http://192.168.1.54:8187/main/calculate-results", request, options);

            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<Dictionary<int, int>>();
        }

    }
    public class PoseidonRequest
    {
        public string[] Inputs { get; set; } = Array.Empty<string>();
    }

    public class PoseidonResponse
    {
        public string Hash { get; set; }
    }
    public class KeysResponse
    {
        public string SK { get; set; }
        public PKey PK { get; set; }
    }
    public class PKey
    {
        public string X { get; set; }
        public string Y { get; set; }

    }
    public class EncodedWeight
    {
        public string Weight { get; set; }
    }
}
