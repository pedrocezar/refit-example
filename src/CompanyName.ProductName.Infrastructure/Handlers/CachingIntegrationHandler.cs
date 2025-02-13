using Microsoft.Extensions.Caching.Memory;

namespace CompanyName.ProductName.Infrastructure.Handlers
{
    public class CachingIntegrationHandler : DelegatingHandler
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration;

        public CachingIntegrationHandler(IMemoryCache cache, TimeSpan? cacheDuration = null)
        {
            _cache = cache;
            _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(10);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method != HttpMethod.Get)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            var cacheKey = GenerateCacheKey(request);

            if (_cache.TryGetValue(cacheKey, out HttpResponseMessage cachedResponse))
            {
                return cachedResponse;
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var clonedResponse = await CloneHttpResponseMessageAsync(response);
                _cache.Set(cacheKey, clonedResponse, _cacheDuration);
            }

            return response;
        }

        private static string GenerateCacheKey(HttpRequestMessage request)
        {
            var uri = request.RequestUri?.ToString() ?? string.Empty;
            return $"CachingHandler_{uri}";
        }

        private static async Task<HttpResponseMessage> CloneHttpResponseMessageAsync(HttpResponseMessage response)
        {
            var newResponse = new HttpResponseMessage(response.StatusCode)
            {
                Version = response.Version,
                RequestMessage = response.RequestMessage,
                ReasonPhrase = response.ReasonPhrase
            };

            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                newResponse.Content = new ByteArrayContent(content);

                foreach (var header in response.Content.Headers)
                {
                    newResponse.Content.Headers.Add(header.Key, header.Value);
                }
            }

            return newResponse;
        }
    }
} 