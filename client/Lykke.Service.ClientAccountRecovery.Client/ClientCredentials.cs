using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client
{
    internal class ClientCredentials : ServiceClientCredentials
    {
        private const string HeaderName = "X-ApiKey";
        private readonly string _apiKey;

        public ClientCredentials(string apiKey)
        {
            _apiKey = apiKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_apiKey != null)
            {
                request.Headers.TryAddWithoutValidation(HeaderName, _apiKey);
            }
            return Task.CompletedTask;
        }
    }
}
