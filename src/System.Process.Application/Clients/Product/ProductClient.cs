using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Worker.Clients.Product
{
    public class ProductClient : IProductClient
    {
        private HttpClient HttpClient { get; }

        public ProductClient(HttpClient httpClient) => HttpClient = httpClient;

        public async Task<ProductMessage> Get(Guid id, CancellationToken cancellation)
        {
            var response = await HttpClient.GetAsync($"product/{id.ToString()}/account", cancellation);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProductMessage>(content);
        }
    }
}
