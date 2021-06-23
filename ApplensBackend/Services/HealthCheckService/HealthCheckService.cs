using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AppLensV3.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private string OutboundConnectivityCheckUrl;
        private static HttpClient _httpClient;
        IConfiguration _configuration;
        bool IsOutboundConnectivityCheckEnabled = false;
        private const string OUTBOUND_CONNECTIVITY_CACHE_KEY = "outboundconnectivitycheck";
        private IMemoryCache cache;

        public HealthCheckService(IConfiguration Configuration, IMemoryCache cache)
        {
            _configuration = Configuration;
            IsOutboundConnectivityCheckEnabled = Convert.ToBoolean(_configuration["HealthCheckSettings:IsOutboundConnectivityCheckEnabled"]);
            if (IsOutboundConnectivityCheckEnabled)
            {
                OutboundConnectivityCheckUrl = _configuration["HealthCheckSettings:OutboundConnectivityCheckUrl"];
                if (OutboundConnectivityCheckUrl != null && OutboundConnectivityCheckUrl.Length > 0)
                {
                    InitializeHttpClient();
                }
                else
                {
                    throw new Exception("Invalid configuration for parameter - HealthCheckSettings:OutboundConnectivityCheckUrl");
                }
            }
            this.cache = cache;
        }


        public async Task RunHealthCheck()
        {
            if (IsOutboundConnectivityCheckEnabled)
            {
                // If cache has a success result, return it
                if (cache.TryGetValue(OUTBOUND_CONNECTIVITY_CACHE_KEY, out bool outboundConnectivityCheck))
                {
                    if (outboundConnectivityCheck)
                        return;
                }
                // Else conduct a check and store it in cache
                bool checkSuccess = await HealthCheckPing();
                if (checkSuccess)
                {
                    var cacheExpirationInSeconds = _configuration.GetValue("HealthCheckSettings:OutboundConnectivityCheckCacheExpirationInSeconds", 120);
                    cache.Set(OUTBOUND_CONNECTIVITY_CACHE_KEY, true, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(cacheExpirationInSeconds)
                    });
                }
                else
                {
                    throw new Exception("Connectivity check failed");
                }
            }
        }

        private async Task<HttpResponseMessage> Get(HttpRequestMessage request)
        {
            //Sleep for a while so that we do not create outbound connections too aggressively causing SNAT port exhaustion.
            await Task.Delay(TimeSpan.FromSeconds(1));
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(3)))
            {
                return await _httpClient.SendAsync(request, cts.Token);
            }
        }

        private async Task<bool> HealthCheckPing()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, OutboundConnectivityCheckUrl);
            var response = await Get(request);
            return response.IsSuccessStatusCode;
        }




        private void InitializeHttpClient()
        {
            _httpClient = new HttpClient();
            _httpClient.MaxResponseContentBufferSize = Int32.MaxValue;
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}
