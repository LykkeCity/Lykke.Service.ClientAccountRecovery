using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    [UsedImplicitly]
    public class StateTableEntity : AzureTableEntity
    {
        public string ClientId => PartitionKey;
        public string RecoveryID => RowKey;

        public static StateTableEntity CreateNew(string clientId, string recoveryId)
        {
            return new StateTableEntity
            {
                PartitionKey = clientId,
                RowKey = recoveryId
            };
        }
    }
}
