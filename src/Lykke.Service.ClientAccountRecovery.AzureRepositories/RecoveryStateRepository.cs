using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    public class RecoveryStateRepository : IRecoveryStateRepository
    {
        private readonly INoSQLTableStorage<StateTableEntity> _storage;

        public RecoveryStateRepository(INoSQLTableStorage<StateTableEntity> storage)
        {
            _storage = storage;
        }

        public Task<IEnumerable<StateTableEntity>> GetAsync(string clientId)
        {
            return _storage.GetDataAsync(clientId); // Expected only a few records per User
        }
        
        public Task InsertOrReplaceAsync(string clientId, string recoveryId)
        {
            var entity = StateTableEntity.CreateNew(clientId, recoveryId);
            return _storage.InsertOrReplaceAsync(entity);
        }

        public Task DeleteAsync(string clientId, string recoveryId)
        {
            return _storage.DeleteIfExistAsync(clientId, recoveryId);
        }
    }
}
