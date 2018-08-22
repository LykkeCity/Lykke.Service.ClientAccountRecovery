using Lykke.Service.ClientAccountRecovery.Core;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    public class WalletCredentialsEntity : TableEntity, IWalletCredentials
    {
        public string Address { get; set; }

        public static class ByClientId
        {
            public static string GeneratePartitionKey()
            {
                return "Wallet";
            }

            public static string GenerateRowKey(string clientId)
            {
                return clientId;
            }
        }
    }
}