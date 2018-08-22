using System;
using System.Net.Http;
using Microsoft.Rest;

namespace Lykke.Service.ClientAccountRecovery.Client.AutoRestClient
{
    public partial class ClientAccountRecoveryServiceClient
    {
        internal ClientAccountRecoveryServiceClient(Uri baseUri, HttpClient client, ServiceClientCredentials credentials) : base(client)
        {
            Credentials = credentials ?? throw new System.ArgumentNullException(nameof(credentials));
            Credentials?.InitializeServiceClient(this);
            Initialize();
            BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
        }
    }
}
