using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using JetBrains.Annotations;
using Lykke.Service.ClientAccountRecovery.Core;
using Lykke.Service.ClientAccountRecovery.Core.Domain;

namespace Lykke.Service.ClientAccountRecovery.AzureRepositories
{
    [UsedImplicitly]
    public class RecoveryLogRepository : IRecoveryLogRepository
    {
        private readonly INoSQLTableStorage<LogTableEntity> _storage;
        private readonly IMapper _mapper;

        public RecoveryLogRepository(INoSQLTableStorage<LogTableEntity> storage, IMapper mapper)
        {
            _storage = storage;
            _mapper = mapper;
        }

        public async Task<RecoveryUnit> GetAsync(string recoveryId)
        {
            var entities = await _storage.GetDataAsync(recoveryId);
            var log = entities.Select(e => _mapper.Map<RecoveryContext>(e));
            return new RecoveryUnit(log.ToArray());
        }


        public Task InsertAsync(RecoveryContext context)
        {
            var entity = _mapper.Map<LogTableEntity>(context);
            return _storage.InsertAsync(entity);
        }

        public Task DeleteAsync(string recoveryId, int seqNo)
        {
            return _storage.DeleteIfExistAsync(LogTableEntity.GetPartitionKey(recoveryId), LogTableEntity.GetRowKey(seqNo));
        }
    }
}
