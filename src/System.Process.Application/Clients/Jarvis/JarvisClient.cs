using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Process.Domain.ValueObjects;

namespace System.Process.Application.Clients.Jarvis
{
    public class JarvisClient : IJarvisClient
    {
        private ILogger<JarvisClient> Logger { get; }
        private HttpClient Client { get; }
        private JarvisConfig Config { get; }

        public JarvisClient(ILogger<JarvisClient> logger,
            HttpClient client,
            IOptions<JarvisConfig> config)
        {
            Logger = logger;
            Client = client;
            Config = config.Value;
        }

        public async Task<DeviceDetails> GetDeviceDetails(string sessionId, CancellationToken cancellationToken)
        {
            try
            {
                var httpResponse = await Client.SendAsync(new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(Config.Url + $"v2/device/{sessionId}")
                }, cancellationToken);

                Logger.LogInformation($"Session Details in Jarvis returned {httpResponse.StatusCode}", httpResponse.ReasonPhrase);

                return JsonConvert.DeserializeObject<DeviceDetails>(await httpResponse.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error to fetch SessionId");
                return new DeviceDetails();
            }
        }
    }
}
